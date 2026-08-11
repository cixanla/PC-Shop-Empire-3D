# PC Shop Empire 1.1.6 — Ownership, Registration, and Trademark Guide

> This document provides general information, not legal advice. Consult a qualified intellectual-property lawyer before commercial publication, collaboration, contractor work, or any disputed ownership claim.

## How cixanla ownership is presented

1. In-game menus and About information display the publisher name `cixanla` and `Copyright © 2026 cixanla. All rights reserved.`
2. Windows product metadata, the package identity, and installer metadata identify cixanla.
3. `GAME_LICENSE.txt` places original project materials under a proprietary license.
4. `THIRD_PARTY_NOTICES.txt` preserves the separate rights governing Electron, Chromium, Node.js, and other components.

These elements provide a public rights notice and release identity. They are not by themselves official registration, trademark registration, or cryptographically verified identity.

## Author and rights-holder distinctions

The Republic of Turkey Ministry of Culture and Tourism describes an author as the person who creates a work; the person or people who write source code are an example. Technical assistance alone does not automatically establish authorship. Economic rights may be transferred through a written agreement. Official reference: [Who is the author?](https://telifhaklari.ktb.gov.tr/TR-332390/eser-sahibi-kimdir.html)

Accordingly:

- If `cixanla` is a pseudonym or brand, use the verified natural-person or legal-entity identity in registrations and contracts where required.
- Record the date and scope of contributions from anyone who creates code, music, art, or text; obtain written assignments or licenses when required.
- Do not claim ownership of third-party or open-source components.
- Rights already granted for copies previously distributed under another license cannot be retroactively withdrawn; clearly label version 1.1.5 and later distributions with the current license.

## Registration in Turkey

### Optional registration

According to the Ministry, optional registration is not mandatory, failure to register does not cause loss of rights, and registration does not create a new right. It is a declaration-based record that can assist in proving who created the work. Current procedure and document requirements:

- [Optional registration — general information](https://telifhaklari.ktb.gov.tr/TR-332370/istege-bagli-kayit-tescil.html)
- [Optional registration — computer programs](https://telifhaklari.ktb.gov.tr/TR-332450/istege-bagli-kayit-tescil.html)

For computer-program applications, the distinction between the source-code author and a person who only acquired economic rights can be important. Follow the Ministry's current guidance and retain concrete contribution records.

### Mandatory registration for computer games

The Ministry's official page lists domestic and imported computer games among works subject to mandatory registration and identifies the Istanbul Copyright and Cinema Directorate as the application authority. Before distribution, confirm the current forms, sample-copy requirements, fees, banderole requirements, and procedure directly from the [mandatory registration page](https://telifhaklari.ktb.gov.tr/TR-332371/zorunlu-kayit-tescil.html).

Fees and required documents can change; do not treat an amount recorded in this repository as permanent application information.

## The cixanla trademark

Copyright and trademark rights are different. Search for similar marks before applying for protection for the game name, publisher name, or logo, then evaluate the appropriate goods/services classes. The [Turkish Patent and Trademark Office](https://www.turkpatent.gov.tr/tr) provides trademark information and electronic filing access.

Class selection, similarity research, and natural-person/legal-entity ownership can materially affect the commercial outcome. Consider advice from a trademark attorney or agent.

## Evidence and release history

Keep the following for every release:

1. A dated source archive and Windows installer
2. SHA-256 checksums for every delivery
3. `CHANGELOG.md`, version number, and release date
4. Design decisions, drafts, task records, and contributor agreements
5. Invoices, licenses, and source references for images, audio, and fonts
6. Signed Git tags and a read-only remote backup where practical

A Git username or copyright line is not a cryptographic signature. For a verifiable release chain, use a Git tag signed with a personal key. For Windows distribution, use a code-signing certificate issued by a trusted certificate authority and a trusted timestamp.

## Authorship and content provenance

- Keep dated source files, contributor records, and written commercial licenses or assignments for commissioned work.
- Do not remove or falsify C2PA / Content Credentials metadata.
- Make only accurate statements about authorship, ownership, and production history.
- Review final registration and commercial-release documents with qualified legal counsel.

## Pre-release checklist

- [ ] The relationship between the verified legal identity and the cixanla brand is documented.
- [ ] Contributor agreements and third-party licenses are collected.
- [ ] Windows executables and the installer are signed with a trusted code-signing certificate.
- [ ] SHA-256 checksums and a dated source archive are retained.
- [ ] Current mandatory computer-game registration requirements are verified.
- [ ] Trademark research/application is evaluated for PC Shop Empire and cixanla.
- [ ] Store-page, privacy, consumer, and age-rating duties are reviewed for the target market.
