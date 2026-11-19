# Decision: browse screen

Reviewed in headless Chrome at 1440px and 390px (`screens/browse-a|b|c.html`). Scores 1 (poor) to 5 (excellent).

| Variant | Main job speed | Clarity | Accessibility | Tone | Total |
|---|---|---|---|---|---|
| A: Rows, lean-back (hero + horizontal rows) | 4 | 4 | 4 | 5 | 17 |
| B: Editorial, card-first (feature + sidebar + filter chips + grid) | 3 | 4 | 4 | 4 | 15 |
| C: Guided (resume first, then pick a mood) | 5 | 5 | 5 | 3 | 18 |

Notes
- **A** feels like a cinema lobby and works with a TV remote (every poster is a focusable link in a row, arrow keys scroll). Weakness: "Continue watching" is just another row; the resume time is hidden in the card.
- **B** reads well on desktop but the grid is slow to scan on a phone and the sidebar collapses below a long feature; filter chips add a step before anything plays.
- **C** is fastest to a playing film (one tap on "Resume at 12:30") and states the device hand-off in words ("Stopped at 12:30 on your phone"). It hides the breadth of the catalogue, which is the distributor's main selling point, and the mood step is a guess we can't back with data yet.

## Pick: A, with C's resume clarity
- Layout from **A**: hero + horizontal rows, lean-back, remote-friendly.
- "Continue watching" moves to the first row directly under the hero and borrows **C**'s copy: each card shows "Resume at 12:30 · 86 min left" and which device it was stopped on.
- Header borrows **B**'s territory line ("Films licensed in Nigeria") so viewers understand why a title is missing.
- Mood picker from C goes to the backlog (needs viewing data first).

Final: `screens/browse.html` (loading / empty / error / success via the state switcher) and the secondary screen `screens/player.html` (loading, resume prompt, playing, stream error, 402 renew).
