import React, { useSyncExternalStore, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import '../style/registration.css'
import { Oval } from 'react-loader-spinner';

function Registration(){

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');

    //On click of back button go to login
    let navigation = useNavigate();
    const goBack = () => {
        let path = '/';
        navigation(path);
    }

    //Register api call
    const register = () => {
        const username = document.querySelector('.js--username-register').value.trim();
        const email =  document.querySelector('.js--email-register').value.trim();
        const password =  document.querySelector('.js--password-register').value.trim();

        setLoading(true);
        fetch("/api/register",
        {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: "POST",
            body: JSON.stringify({username: username, email: email, pwd: password })
        })
        .then(async function(res) {
            if(res.ok){
                setLoading(false);
                let path = '/login';
                navigation(path);
            } else{
                const errorData = await res.json();
                console.log(errorData);
                setError(true);
                setErrorMsg(errorData.error_msg);
            }
        })
        .catch( err => 
            console.log('Something went wrong')
        );
    }

    return(
    <div className="container">
        <div className='wrapper'>
            <div className="back-button-container">
                <button className="back-button action-button" onClick={goBack}>Back</button>
            </div>
            <div className="registration-form">

                <input type="text" placeholder='Username' className="input-field form-item js--username-register" />
                <input type="text" placeholder='E-mail' className="input-field form-item js--email-register" />
                <input type="password" placeholder='Password' className="input-field form-item js--password-register" />
                {error
                        ? <p className="form-item text-field">{errorMsg}</p>
                        : <p></p>
                    }
                
                <button className="action-button form-item" onClick={register}>Register</button>
            </div>
        </div>
    </div>
    );
}

export default Registration;
