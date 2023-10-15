import { BrowserRouter as Router, Routes, Route } from "react-router-dom"
import { UserContext, UserProvider } from "./userContext";

import logo from './logo.svg';
import './style/style.css';
import Registration from './components/Registration';
import Login from './components/Login.js';
import Timeline from "./components/Timeline";
import Navbar from "./components/Navbar";

function App() {
  return (
    <div className="App">
        <Router>
          <UserProvider>
            <Navbar />
            <Routes>
                <Route exact path="/Login" element={<Login />} />
                <Route path="/Register" element={<Registration />} />
                <Route path="/" element={<Timeline />} />
            </Routes>
          </UserProvider>
        </Router>
    </div>
  );
}

export default App;
