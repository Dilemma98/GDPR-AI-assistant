import React from 'react';
import "./app.css";
import Header from "./components/header";
import ChatView from "./components/chatView";


function App() {
  return (
    <div className="App">
      <Header />
      <ChatView />
    </div>
  );
}

export default App;
