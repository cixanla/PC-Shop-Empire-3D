using System;
using System.Collections.Generic;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// Owns accepted custom-PC requests and their immutable quote/BOM snapshots. Inventory
    /// remains the single stock authority; this aggregate only coordinates its atomic exact
    /// serialized reservation-set boundary.
    /// </summary>
    public sealed class CustomPcQuoteAuthority
    {
        public const int GraphicsFirstGamingLineCount = 10;

        private readonly ProductCatalog _catalog;
        private readonly PcComponentCatalog _components;
        private readonly InventoryAuthority _inventory;
        private readonly CustomerConsultationAuthority _consultations;
        private readonly Dictionary<StableId<CustomPcRequestIdScope>, CustomPcRequestRecord>
            _requests =
                new Dictionary<StableId<CustomPcRequestIdScope>, CustomPcRequestRecord>();
        private readonly Dictionary<StableId<CustomerConsultationIdScope>, CustomPcRequestRecord>
            _requestsByConsultation =
                new Dictionary<StableId<CustomerConsultationIdScope>, CustomPcRequestRecord>();
        private readonly Dictionary<StableId<CustomPcQuoteIdScope>, CustomPcQuoteRecord> _quotes =
            new Dictionary<StableId<CustomPcQuoteIdScope>, CustomPcQuoteRecord>();
        private readonly Dictionary<StableId<CustomPcRequestIdScope>, CustomPcQuoteRecord>
            _quotesByRequest =
                new Dictionary<StableId<CustomPcRequestIdScope>, CustomPcQuoteRecord>();
        private readonly Dictionary<StableId<InventoryClaimIdScope>, CustomPcQuoteRecord>
            _quotesByInventoryClaim =
                new Dictionary<StableId<InventoryClaimIdScope>, CustomPcQuoteRecord>();
        private readonly Dictionary<StableId<CustomPcQuoteIdScope>,
            InventorySerializedReservationSetAccess> _reservationSetsByQuote =
                new Dictionary<StableId<CustomPcQuoteIdScope>,
                    InventorySerializedReservationSetAccess>();

        private CustomPcQuoteAuthority(
            ProductCatalog catalog,
            PcComponentCatalog components,
            InventoryAuthority inventory,
            CustomerConsultationAuthority consultations)
        {
            _catalog = catalog;
            _components = components;
            _inventory = inventory;
            _consultations = consultations;
        }

        public long Revision { get; private set; }

        public int RequestCount => _requests.Count;

        public int QuoteCount => _quotes.Count;

        public ProductCatalog Catalog => _catalog;

        public PcComponentCatalog Components => _components;

        public InventoryAuthority Inventory => _inventory;

        public CustomerConsultationAuthority Consultations => _consultations;

        public static OperationResult<CustomPcQuoteAuthority> Create(
            ProductCatalog catalog,
            PcComponentCatalog components,
            InventoryAuthority inventory,
            CustomerConsultationAuthority consultations)
        {
            if (catalog == null ||
                components == null ||
                inventory == null ||
                consultations == null)
            {
                return OperationResult<CustomPcQuoteAuthority>.Fail(
                    CustomPcQuoteFailures.MissingAuthority);
            }

            return !ReferenceEquals(components.OwnerCatalog, catalog) ||
                   !inventory.UsesCatalog(catalog)
                ? OperationResult<CustomPcQuoteAuthority>.Fail(
                    CustomPcQuoteFailures.AuthorityMismatch)
                : OperationResult<CustomPcQuoteAuthority>.Success(
                    new CustomPcQuoteAuthority(
                        catalog,
                        components,
                        inventory,
                        consultations));
        }

        public OperationResult AcceptRequest(
            StableId<CustomPcRequestIdScope> requestId,
            CustomerRetailIdentityBinding customerBinding,
            CustomerConsultationRecord consultation,
            CustomPcBuildProfile profile,
            ShelfPrice maximumBudget,
            SimulationTimestamp acceptedAt)
        {
            Failure failure = ValidateRequestInput(
                requestId,
                customerBinding,
                consultation,
                profile,
                maximumBudget,
                acceptedAt);
            if (!failure.IsNone)
            {
                return OperationResult.Fail(failure);
            }

            if (_requests.TryGetValue(requestId, out CustomPcRequestRecord existing))
            {
                return existing.Matches(
                    customerBinding,
                    consultation,
                    profile,
                    maximumBudget,
                    acceptedAt)
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        CustomPcQuoteFailures.RequestIdentityConflict);
            }

            if (_requestsByConsultation.ContainsKey(consultation.Id))
            {
                return OperationResult.Fail(
                    CustomPcQuoteFailures.ConsultationAlreadyAccepted);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(CustomPcQuoteFailures.RevisionOverflow);
            }

            var request = new CustomPcRequestRecord(
                requestId,
                customerBinding,
                consultation,
                profile,
                maximumBudget,
                acceptedAt);
            _requests.Add(requestId, request);
            _requestsByConsultation.Add(consultation.Id, request);
            Revision++;
            return OperationResult.Success();
        }

        public OperationResult CreateQuoteAndReserve(
            StableId<CustomPcQuoteIdScope> quoteId,
            StableId<CustomPcRequestIdScope> requestId,
            StableId<InventoryClaimIdScope> inventoryClaimId,
            IReadOnlyList<CustomPcQuoteLineDraft> lines,
            SimulationTimestamp quotedAt)
        {
            if (quoteId.IsEmpty || requestId.IsEmpty || inventoryClaimId.IsEmpty)
            {
                return OperationResult.Fail(CustomPcQuoteFailures.InputInvalid);
            }

            if (!_requests.TryGetValue(requestId, out CustomPcRequestRecord request))
            {
                return OperationResult.Fail(CustomPcQuoteFailures.UnknownRequest);
            }

            OperationResult<CustomPcQuoteRecord> preparedQuote = PrepareQuoteSnapshot(
                quoteId,
                request,
                inventoryClaimId,
                lines,
                quotedAt,
                out IReadOnlyList<InventorySerializedReservationRequest>
                    reservationRequests);
            if (preparedQuote.IsFailure)
            {
                return OperationResult.Fail(preparedQuote.Error);
            }

            CustomPcQuoteRecord candidate = preparedQuote.Value;
            if (_quotes.TryGetValue(quoteId, out CustomPcQuoteRecord existing))
            {
                return existing.HasExactIdentity(candidate) &&
                       HasExactLiveReservations(existing)
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        existing.HasExactIdentity(candidate)
                            ? CustomPcQuoteFailures.InventoryReservationDrift
                            : CustomPcQuoteFailures.QuoteIdentityConflict);
            }

            if (_quotesByRequest.ContainsKey(requestId))
            {
                return OperationResult.Fail(CustomPcQuoteFailures.RequestAlreadyQuoted);
            }

            if (_quotesByInventoryClaim.ContainsKey(inventoryClaimId))
            {
                return OperationResult.Fail(
                    CustomPcQuoteFailures.InventoryClaimAlreadyQuoted);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(CustomPcQuoteFailures.RevisionOverflow);
            }

            OperationResult<InventorySerializedReservationSetAccess> inventorySet =
                _inventory.ReserveManagedSerializedItems(
                    CreateInventoryReservationSetOperationId(quoteId),
                    reservationRequests);
            if (inventorySet.IsFailure)
            {
                return OperationResult.Fail(inventorySet.Error);
            }

            _quotes.Add(quoteId, candidate);
            _quotesByRequest.Add(requestId, candidate);
            _quotesByInventoryClaim.Add(inventoryClaimId, candidate);
            _reservationSetsByQuote.Add(quoteId, inventorySet.Value);
            Revision++;
            return OperationResult.Success();
        }

        internal static StableId<InventorySerializedReservationSetOperationIdScope>
            CreateInventoryReservationSetOperationId(
                StableId<CustomPcQuoteIdScope> quoteId)
        {
            return StableId<InventorySerializedReservationSetOperationIdScope>.Parse(
                $"inventory.serialized-reservation-set.custom-pc.{quoteId.Value}");
        }

        public bool TryGetRequest(
            StableId<CustomPcRequestIdScope> requestId,
            out CustomPcRequestRecord request)
        {
            return _requests.TryGetValue(requestId, out request);
        }

        public bool TryGetQuote(
            StableId<CustomPcQuoteIdScope> quoteId,
            out CustomPcQuoteRecord quote)
        {
            return _quotes.TryGetValue(quoteId, out quote);
        }

        public bool TryGetQuoteForRequest(
            StableId<CustomPcRequestIdScope> requestId,
            out CustomPcQuoteRecord quote)
        {
            return _quotesByRequest.TryGetValue(requestId, out quote);
        }

        public IReadOnlyList<CustomPcRequestRecord> GetRequests()
        {
            var values = new List<CustomPcRequestRecord>(_requests.Values);
            values.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public IReadOnlyList<CustomPcQuoteRecord> GetQuotes()
        {
            var values = new List<CustomPcQuoteRecord>(_quotes.Values);
            values.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            if (_catalog == null ||
                _components == null ||
                _inventory == null ||
                _consultations == null ||
                !ReferenceEquals(_components.OwnerCatalog, _catalog) ||
                !_inventory.UsesCatalog(_catalog) ||
                _inventory.ValidateInvariants().IsFailure ||
                _consultations.ValidateInvariants().IsFailure ||
                Revision < 0 ||
                _requests.Count != _requestsByConsultation.Count ||
                _quotes.Count != _quotesByRequest.Count ||
                _quotes.Count != _quotesByInventoryClaim.Count ||
                _quotes.Count != _reservationSetsByQuote.Count)
            {
                return OperationResult.Fail(CustomPcQuoteFailures.InvariantViolation);
            }

            foreach (KeyValuePair<StableId<CustomPcRequestIdScope>,
                CustomPcRequestRecord> entry in _requests)
            {
                CustomPcRequestRecord request = entry.Value;
                if (request == null ||
                    entry.Key != request.Id ||
                    !HasValidRequest(request) ||
                    !_requestsByConsultation.TryGetValue(
                        request.Consultation.Id,
                        out CustomPcRequestRecord byConsultation) ||
                    !ReferenceEquals(request, byConsultation))
                {
                    return OperationResult.Fail(
                        CustomPcQuoteFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<StableId<CustomPcQuoteIdScope>,
                CustomPcQuoteRecord> entry in _quotes)
            {
                CustomPcQuoteRecord quote = entry.Value;
                if (quote == null ||
                    entry.Key != quote.Id ||
                    quote.Request == null ||
                    !_requests.TryGetValue(
                        quote.Request.Id,
                        out CustomPcRequestRecord request) ||
                    !ReferenceEquals(request, quote.Request) ||
                    !_quotesByRequest.TryGetValue(
                        request.Id,
                        out CustomPcQuoteRecord byRequest) ||
                    !ReferenceEquals(quote, byRequest) ||
                    !_quotesByInventoryClaim.TryGetValue(
                        quote.InventoryClaimId,
                        out CustomPcQuoteRecord byClaim) ||
                    !ReferenceEquals(quote, byClaim) ||
                    !_reservationSetsByQuote.ContainsKey(quote.Id) ||
                    !HasValidQuote(quote) ||
                    !HasExactLiveReservations(quote))
                {
                    return OperationResult.Fail(
                        CustomPcQuoteFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private Failure ValidateRequestInput(
            StableId<CustomPcRequestIdScope> requestId,
            CustomerRetailIdentityBinding customerBinding,
            CustomerConsultationRecord consultation,
            CustomPcBuildProfile profile,
            ShelfPrice maximumBudget,
            SimulationTimestamp acceptedAt)
        {
            if (requestId.IsEmpty ||
                customerBinding == null ||
                consultation == null ||
                !IsValidProfile(profile) ||
                !IsValidPrice(maximumBudget))
            {
                return CustomPcQuoteFailures.InputInvalid;
            }

            if (!_consultations.Owns(consultation) ||
                customerBinding.ActorCustomerId != consultation.CustomerId)
            {
                return CustomPcQuoteFailures.ConsultationMismatch;
            }

            if (consultation.Need != CustomerNeedKind.GraphicsUpgrade)
            {
                return CustomPcQuoteFailures.NeedUnsupported;
            }

            return acceptedAt.IsAtOrAfter(consultation.RecordedAt)
                ? Failure.None
                : CustomPcQuoteFailures.TimestampInvalid;
        }

        private OperationResult<CustomPcQuoteRecord> PrepareQuoteSnapshot(
            StableId<CustomPcQuoteIdScope> quoteId,
            CustomPcRequestRecord request,
            StableId<InventoryClaimIdScope> inventoryClaimId,
            IReadOnlyList<CustomPcQuoteLineDraft> lineDrafts,
            SimulationTimestamp quotedAt,
            out IReadOnlyList<InventorySerializedReservationRequest>
                reservationRequests)
        {
            reservationRequests = null;
            if (lineDrafts == null)
            {
                return OperationResult<CustomPcQuoteRecord>.Fail(
                    CustomPcQuoteFailures.MissingLines);
            }

            if (lineDrafts.Count == 0)
            {
                return OperationResult<CustomPcQuoteRecord>.Fail(
                    CustomPcQuoteFailures.EmptyLines);
            }

            if (!quotedAt.IsAtOrAfter(request.AcceptedAt))
            {
                return OperationResult<CustomPcQuoteRecord>.Fail(
                    CustomPcQuoteFailures.TimestampInvalid);
            }

            var lineIds = new HashSet<StableId<CustomPcBomLineIdScope>>();
            var productIds = new HashSet<StableId<ProductDefinitionIdScope>>();
            var itemIds = new HashSet<StableId<ItemInstanceIdScope>>();
            var reservationIds = new HashSet<StableId<ReservationIdScope>>();
            var snapshots = new List<CustomPcQuoteLineSnapshot>(lineDrafts.Count);
            var exactReservations =
                new List<InventorySerializedReservationRequest>(lineDrafts.Count);
            long totalMinorUnits = 0;

            for (int index = 0; index < lineDrafts.Count; index++)
            {
                CustomPcQuoteLineDraft draft = lineDrafts[index];
                if (draft == null)
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        CustomPcQuoteFailures.NullLine);
                }

                if (!lineIds.Add(draft.LineId) ||
                    !productIds.Add(draft.ProductId) ||
                    !itemIds.Add(draft.ItemId) ||
                    !reservationIds.Add(draft.ReservationId))
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        CustomPcQuoteFailures.DuplicateLineIdentity);
                }

                if (!_catalog.TryGet(draft.ProductId, out ProductDefinition product) ||
                    product.TrackingPolicy != ProductTrackingPolicy.SerializedInstance ||
                    !_components.TryGet(
                        draft.ProductId,
                        out PcComponentSpecification component))
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        CustomPcQuoteFailures.UnknownComponent);
                }

                if (!_inventory.TryGetSerializedItem(
                        draft.ItemId,
                        out InventoryItemRecord item))
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        InventoryFailures.UnknownItem);
                }

                if (item.ProductId != draft.ProductId)
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        CustomPcQuoteFailures.ItemProductMismatch);
                }

                if (item.Condition != InventoryCondition.New)
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        CustomPcQuoteFailures.ItemConditionUnsupported);
                }

                if (!IsValidPrice(draft.UnitPrice) ||
                    draft.UnitPrice.Currency != request.MaximumBudget.Currency ||
                    !string.Equals(
                        item.UnitCost.CurrencyCode,
                        request.MaximumBudget.Currency.Value,
                        StringComparison.Ordinal))
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        CustomPcQuoteFailures.CurrencyMismatch);
                }

                if (long.MaxValue - totalMinorUnits < draft.UnitPrice.MinorUnits ||
                    ShelfPrice.MaximumMinorUnits - totalMinorUnits <
                        draft.UnitPrice.MinorUnits)
                {
                    return OperationResult<CustomPcQuoteRecord>.Fail(
                        CustomPcQuoteFailures.TotalOverflow);
                }

                totalMinorUnits += draft.UnitPrice.MinorUnits;
                snapshots.Add(new CustomPcQuoteLineSnapshot(
                    draft.LineId,
                    draft.ProductId,
                    draft.ItemId,
                    draft.ReservationId,
                    component.Kind,
                    component.PowerCableType,
                    item.UnitCost,
                    draft.UnitPrice));
                exactReservations.Add(
                    InventorySerializedReservationRequest.Create(
                        draft.ReservationId,
                        inventoryClaimId,
                        draft.ItemId).Value);
            }

            Failure componentFailure = ValidateComponentSet(snapshots);
            if (!componentFailure.IsNone)
            {
                return OperationResult<CustomPcQuoteRecord>.Fail(componentFailure);
            }

            if (totalMinorUnits > request.MaximumBudget.MinorUnits)
            {
                return OperationResult<CustomPcQuoteRecord>.Fail(
                    CustomPcQuoteFailures.BudgetExceeded);
            }

            snapshots.Sort((left, right) => string.Compare(
                left.LineId.Value,
                right.LineId.Value,
                StringComparison.Ordinal));
            exactReservations.Sort((left, right) => string.Compare(
                left.ReservationId.Value,
                right.ReservationId.Value,
                StringComparison.Ordinal));
            reservationRequests = Array.AsReadOnly(exactReservations.ToArray());
            ShelfPrice total = ShelfPrice.Create(
                request.MaximumBudget.Currency.Value,
                totalMinorUnits).Value;
            return OperationResult<CustomPcQuoteRecord>.Success(
                new CustomPcQuoteRecord(
                    quoteId,
                    request,
                    inventoryClaimId,
                    quotedAt,
                    total,
                    Array.AsReadOnly(snapshots.ToArray())));
        }

        private Failure ValidateComponentSet(
            IReadOnlyList<CustomPcQuoteLineSnapshot> lines)
        {
            if (lines == null || lines.Count != GraphicsFirstGamingLineCount)
            {
                return CustomPcQuoteFailures.ComponentSetInvalid;
            }

            PcComponentSpecification motherboard = null;
            PcComponentSpecification processor = null;
            PcComponentSpecification memory = null;
            PcComponentSpecification storage = null;
            PcComponentSpecification cooler = null;
            PcComponentSpecification graphics = null;
            PcComponentSpecification powerSupply = null;
            var cables = new HashSet<PowerCableType>();

            for (int index = 0; index < lines.Count; index++)
            {
                if (!_components.TryGet(
                        lines[index].ProductId,
                        out PcComponentSpecification component))
                {
                    return CustomPcQuoteFailures.UnknownComponent;
                }

                switch (component.Kind)
                {
                    case PcComponentKind.Motherboard:
                        if (motherboard != null) return CustomPcQuoteFailures.ComponentSetInvalid;
                        motherboard = component;
                        break;
                    case PcComponentKind.Processor:
                        if (processor != null) return CustomPcQuoteFailures.ComponentSetInvalid;
                        processor = component;
                        break;
                    case PcComponentKind.MemoryModule:
                        if (memory != null) return CustomPcQuoteFailures.ComponentSetInvalid;
                        memory = component;
                        break;
                    case PcComponentKind.StorageDevice:
                        if (storage != null) return CustomPcQuoteFailures.ComponentSetInvalid;
                        storage = component;
                        break;
                    case PcComponentKind.ProcessorCooler:
                        if (cooler != null) return CustomPcQuoteFailures.ComponentSetInvalid;
                        cooler = component;
                        break;
                    case PcComponentKind.GraphicsCard:
                        if (graphics != null) return CustomPcQuoteFailures.ComponentSetInvalid;
                        graphics = component;
                        break;
                    case PcComponentKind.PowerSupply:
                        if (powerSupply != null) return CustomPcQuoteFailures.ComponentSetInvalid;
                        powerSupply = component;
                        break;
                    case PcComponentKind.PowerCable:
                        if (!cables.Add(component.PowerCableType))
                        {
                            return CustomPcQuoteFailures.ComponentSetInvalid;
                        }
                        break;
                    default:
                        return CustomPcQuoteFailures.ComponentSetInvalid;
                }
            }

            if (motherboard == null ||
                processor == null ||
                memory == null ||
                storage == null ||
                cooler == null ||
                graphics == null ||
                powerSupply == null ||
                cables.Count != 3 ||
                !cables.Contains(
                    PowerCableType.ModularAtx24SplitPsuToMotherboard) ||
                !cables.Contains(
                    PowerCableType.ModularEps12v8PinPsuToMotherboard) ||
                !cables.Contains(
                    PowerCableType.ModularPcie8PinPsuToGraphicsCard))
            {
                return CustomPcQuoteFailures.ComponentSetInvalid;
            }

            return motherboard.CpuSocketFamily != processor.CpuSocketFamily ||
                   motherboard.CpuSocketFamily != cooler.CpuSocketFamily ||
                   motherboard.DimmType != memory.DimmType ||
                   motherboard.M2StorageType != storage.M2StorageType ||
                   motherboard.GraphicsCardType != graphics.GraphicsCardType ||
                   !PcComponentSpecification.IsValidPowerSupplyType(
                       powerSupply.PowerSupplyType)
                ? CustomPcQuoteFailures.ComponentIncompatible
                : Failure.None;
        }

        private bool HasExactLiveReservations(CustomPcQuoteRecord quote)
        {
            if (quote == null ||
                quote.Lines == null ||
                quote.Lines.Count == 0 ||
                !_reservationSetsByQuote.TryGetValue(
                    quote.Id,
                    out InventorySerializedReservationSetAccess access) ||
                access.ManagedClaimId != quote.InventoryClaimId ||
                access.Requests == null ||
                access.Requests.Count != quote.Lines.Count ||
                !_inventory.OwnsManagedSerializedReservationSet(access))
            {
                return false;
            }

            var requests = new Dictionary<StableId<ReservationIdScope>,
                InventorySerializedReservationRequest>();
            for (int index = 0; index < access.Requests.Count; index++)
            {
                InventorySerializedReservationRequest request = access.Requests[index];
                if (request == null ||
                    request.ClaimId != quote.InventoryClaimId ||
                    requests.ContainsKey(request.ReservationId))
                {
                    return false;
                }

                requests.Add(request.ReservationId, request);
            }

            for (int index = 0; index < quote.Lines.Count; index++)
            {
                CustomPcQuoteLineSnapshot line = quote.Lines[index];
                if (line == null ||
                    !requests.TryGetValue(
                        line.ReservationId,
                        out InventorySerializedReservationRequest request) ||
                    request.ItemId != line.ItemId ||
                    !_inventory.TryGetReservation(
                        line.ReservationId,
                        out InventoryReservation reservation) ||
                    reservation.TargetKind !=
                        InventoryReservationTargetKind.SerializedItem ||
                    reservation.ClaimId != quote.InventoryClaimId ||
                    reservation.ItemId != line.ItemId ||
                    reservation.Quantity != 1 ||
                    reservation.ReleasePolicy !=
                        InventoryReservationReleasePolicy.Releasable ||
                    !_inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item) ||
                    item.ProductId != line.ProductId)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasValidRequest(CustomPcRequestRecord request)
        {
            return request != null &&
                   !request.Id.IsEmpty &&
                   request.CustomerBinding != null &&
                   request.Consultation != null &&
                   _consultations.Owns(request.Consultation) &&
                   request.CustomerBinding.ActorCustomerId ==
                       request.Consultation.CustomerId &&
                   request.Consultation.Need == CustomerNeedKind.GraphicsUpgrade &&
                   IsValidProfile(request.Profile) &&
                   IsValidPrice(request.MaximumBudget) &&
                   request.AcceptedAt.IsAtOrAfter(
                       request.Consultation.RecordedAt);
        }

        private bool HasValidQuote(CustomPcQuoteRecord quote)
        {
            if (quote == null ||
                quote.Id.IsEmpty ||
                quote.Request == null ||
                quote.InventoryClaimId.IsEmpty ||
                !quote.QuotedAt.IsAtOrAfter(quote.Request.AcceptedAt) ||
                !IsValidPrice(quote.TotalPrice) ||
                quote.TotalPrice.Currency != quote.Request.MaximumBudget.Currency ||
                quote.TotalPrice.MinorUnits >
                    quote.Request.MaximumBudget.MinorUnits ||
                quote.Lines == null ||
                quote.Lines.Count != GraphicsFirstGamingLineCount)
            {
                return false;
            }

            long total = 0;
            string previousLineId = null;
            for (int index = 0; index < quote.Lines.Count; index++)
            {
                CustomPcQuoteLineSnapshot line = quote.Lines[index];
                if (line == null ||
                    line.LineId.IsEmpty ||
                    line.ProductId.IsEmpty ||
                    line.ItemId.IsEmpty ||
                    line.ReservationId.IsEmpty ||
                    !line.UnitCost.IsValid ||
                    !IsValidPrice(line.UnitPrice) ||
                    line.UnitPrice.Currency != quote.TotalPrice.Currency ||
                    (previousLineId != null &&
                     string.Compare(
                         previousLineId,
                         line.LineId.Value,
                         StringComparison.Ordinal) >= 0) ||
                    long.MaxValue - total < line.UnitPrice.MinorUnits)
                {
                    return false;
                }

                previousLineId = line.LineId.Value;
                total += line.UnitPrice.MinorUnits;
            }

            return total == quote.TotalPrice.MinorUnits &&
                   ValidateComponentSet(quote.Lines).IsNone;
        }

        private static bool IsValidProfile(CustomPcBuildProfile profile)
        {
            return profile == CustomPcBuildProfile.GraphicsFirstGaming;
        }

        private static bool IsValidPrice(ShelfPrice price)
        {
            return CurrencyCode.IsValid(price.Currency.Value) &&
                   price.MinorUnits > 0 &&
                   price.MinorUnits <= ShelfPrice.MaximumMinorUnits;
        }
    }

    public static class CustomPcQuoteFailures
    {
        public static readonly Failure MissingAuthority =
            Failure.FromCode("retail.custom-pc.authority-missing");
        public static readonly Failure AuthorityMismatch =
            Failure.FromCode("retail.custom-pc.authority-mismatch");
        public static readonly Failure InputInvalid =
            Failure.FromCode("retail.custom-pc.input-invalid");
        public static readonly Failure ConsultationMismatch =
            Failure.FromCode("retail.custom-pc.consultation-mismatch");
        public static readonly Failure ConsultationAlreadyAccepted =
            Failure.FromCode("retail.custom-pc.consultation-already-accepted");
        public static readonly Failure NeedUnsupported =
            Failure.FromCode("retail.custom-pc.need-unsupported");
        public static readonly Failure TimestampInvalid =
            Failure.FromCode("retail.custom-pc.timestamp-invalid");
        public static readonly Failure RequestIdentityConflict =
            Failure.FromCode("retail.custom-pc.request-identity-conflict");
        public static readonly Failure UnknownRequest =
            Failure.FromCode("retail.custom-pc.request-unknown");
        public static readonly Failure RequestAlreadyQuoted =
            Failure.FromCode("retail.custom-pc.request-already-quoted");
        public static readonly Failure InventoryClaimAlreadyQuoted =
            Failure.FromCode("retail.custom-pc.inventory-claim-already-quoted");
        public static readonly Failure QuoteIdentityConflict =
            Failure.FromCode("retail.custom-pc.quote-identity-conflict");
        public static readonly Failure MissingLines =
            Failure.FromCode("retail.custom-pc.lines-missing");
        public static readonly Failure EmptyLines =
            Failure.FromCode("retail.custom-pc.lines-empty");
        public static readonly Failure NullLine =
            Failure.FromCode("retail.custom-pc.line-null");
        public static readonly Failure InvalidLine =
            Failure.FromCode("retail.custom-pc.line-invalid");
        public static readonly Failure DuplicateLineIdentity =
            Failure.FromCode("retail.custom-pc.line-identity-duplicate");
        public static readonly Failure UnknownComponent =
            Failure.FromCode("retail.custom-pc.component-unknown");
        public static readonly Failure ComponentSetInvalid =
            Failure.FromCode("retail.custom-pc.component-set-invalid");
        public static readonly Failure ComponentIncompatible =
            Failure.FromCode("retail.custom-pc.component-incompatible");
        public static readonly Failure ItemProductMismatch =
            Failure.FromCode("retail.custom-pc.item-product-mismatch");
        public static readonly Failure ItemConditionUnsupported =
            Failure.FromCode("retail.custom-pc.item-condition-unsupported");
        public static readonly Failure CurrencyMismatch =
            Failure.FromCode("retail.custom-pc.currency-mismatch");
        public static readonly Failure BudgetExceeded =
            Failure.FromCode("retail.custom-pc.budget-exceeded");
        public static readonly Failure TotalOverflow =
            Failure.FromCode("retail.custom-pc.total-overflow");
        public static readonly Failure InventoryReservationDrift =
            Failure.FromCode("retail.custom-pc.inventory-reservation-drift");
        public static readonly Failure RevisionOverflow =
            Failure.FromCode("retail.custom-pc.revision-overflow");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("retail.custom-pc.invariant");
    }
}
