import React, {useState} from "react";
import { useNavigate } from 'react-router-dom';
import '../style/navbar.css';

function Navbar() {
    //Nav scripts
    let navigation = useNavigate();

    const goToLogin = () => {
        let path = '/Login';
        navigation(path);
    };

    const goToTimeline = () => {
        let path = '/';
        navigation(path);
    }


    return (
        <div className="nav-bar">
            
            <div className="btn-link">
                <button className="action-btn">
                   My profile 
                </button>
            </div>
            
            <div className="btn-link">
                <button className="action-btn" onClick={goToTimeline}>
                    Timeline
                </button>
            </div>
            
            <div className="btn-link login-btn">
                <button className="action-btn" onClick={goToLogin}>
                   Login 
                </button>
            </div>

        </div>
    );
}

export default Navbar;
