import { useState, useRef, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import "./chatView.css";

interface Message {
  role: "user" | "assistant";
  content: string;
  timeSent: string;
}

export default function ChatView() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  const now = new Date().toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });

  const sendMessage = async () => {
    if (!input.trim()) return;

    const userMessage: Message = {
      role: "user",
      content: input,
      timeSent: now,
    };
    setMessages((prev) => [...prev, userMessage]);
    setInput("");
    setIsLoading(true);

    try {
      const response = await fetch("http://localhost:5209/api/gdpr/ask", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(input),
      });

      const data = await response.json();
      const assistantMessage: Message = {
        role: "assistant",
        content: data.answer,
        timeSent: now,
      };
      setMessages((prev) => [...prev, assistantMessage]);
    } catch (error) {
      console.error("Fel: ", error);
    } finally {
      setIsLoading(false);
    }
  };

  const timeNow = new Date().toLocaleTimeString();

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  };

  return (
    <div className="chatContainer">
      <div className="messages">
        {messages.length === 0 && (
          <p className="placeholder">Ställ din fråga...</p>
        )}
        {messages.map((msg, i) => (
          <div key={i} className={`message ${msg.role}`}>
            <span className="label">{msg.role === "user" ? "" : "AI"} · {now} </span>
            <ReactMarkdown>{msg.content}</ReactMarkdown>
          </div>
        ))}
        {isLoading && (
          <div className="message assistant">
            <div className="meta">AI-assistent · {timeNow}</div>
            <p className="loading">Tänker...</p>
          </div>
        )}
         <div ref={messagesEndRef} />
        <div className="input-area">
          <textarea
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Skriv din GDPR-fråga här..."
            rows={3}
          />
          <button onClick={sendMessage} disabled={isLoading}>
            Skicka
          </button>
        </div>
      </div>
    </div>
  );
}
