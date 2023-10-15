import {createContext, useState} from 'react'

const UserContext = createContext(null);

function UserProvider({ children }) {

    const [loggedUser, setUser] = useState(null);

    const login = (userData) => {
      setUser(userData);
    };

    const logout = () => {
      setUser(null);
    };


    return <UserContext.Provider value={{ loggedUser, login, logout }}>{children}</UserContext.Provider>;
}

export { UserContext as default, UserProvider }
