import "./header.css";

export default function Header() {
  return (
    <header className="top">
      <div className="header-left">
        <div className="icon">🔒</div>
        <div>
          <h1>Din AI-assistent inom GDPR-frågor</h1>
          <p>
            Ställ frågor om GDPR baserat på den officiella förordningstexten
          </p>
        </div>
      </div>

      <div className="header-info">
        <strong>Källan är GDPR-förordningen (EU) 2016/679</strong>
        <p>
          Svaren baseras på den officiella förordningstexten. Informationen är
          allmän och utgör inte juridisk rådgivning.
        </p>
      </div>
    </header>
  );
}
