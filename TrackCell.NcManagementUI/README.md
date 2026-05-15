# TrackCell.NcManagementUI

Standalone Angular UI for the TrackCell Non-Conformance (NC) Management system.
Split out from `TrackCell.AngularUI` so that the NC workflow (part editor,
inspector, dashboard, tickets) can be developed and deployed independently of
the rest of the MES front end.

## Pages

The app uses a left-side navigation with four pages:

1. **Part Editor** — Engineers upload part sketches, draw zones, and select
   allowed non-conformance types per zone.
2. **Inspector** — Inspectors enter a serial number, pick an operation, and
   either mark the operation OK or record findings against zones.
3. **Reporting Dashboard** — Aggregate views: NC counts/rates by part, zone,
   operation, NC type, inspector, shift, and date range.
4. **Tickets** — Auto-generated tickets when 5 consecutive NCs occur on the
   same (Part, Zone). Open → InProgress → Resolved → Closed.

The Part Editor is fully wired up against the existing TrackCell API
endpoints (`/MasterData`, `/NonConformances`, `/PartImages`). The Inspector,
Dashboard and Tickets pages are scaffolded as placeholders ready to be filled
in.

## Development server

```bash
npm install
npm start
```

Then open `http://localhost:4200/`.

## Building

```bash
npm run build
```

Build artifacts land in `dist/`.

## API base URL

The API base URL lives in `src/environments/environment.ts` (and
`environment.prod.ts`). Default is `http://localhost:5016`.
