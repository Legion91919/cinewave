export function PaywallNotice({ renewUrl, detail, onBack }: { renewUrl: string; detail: string; onBack: () => void }) {
  return (
    <div className="overlay">
      <div className="dialog" role="alertdialog" aria-labelledby="pw-h" aria-describedby="pw-d">
        <h2 id="pw-h">Your subscription has ended</h2>
        <p className="meta" id="pw-d">{detail} Your place in every film is kept. Card details are entered on our payment partner's secure page, never on Cinewave.</p>
        <div className="actions">
          <a className="btn primary" href={renewUrl}>Renew subscription</a>
          <button className="btn" onClick={onBack}>Back to browse</button>
        </div>
      </div>
    </div>
  );
}
