import React, { useContext } from 'react';
import { useEffect, useState } from 'react';
import '../style/timeline.css'
import  UserContext  from '../userContext';
import { CgProfile } from 'react-icons/cg';
import { Oval } from 'react-loader-spinner';

function Timeline(){

    const [msgs, setMsgs] = useState([]);
    const [followsMap, setFollowsMap] = useState(new Map());
    const [loading, setLoading] = useState(true);
    const updateFollowsMap = (k,v) => {
        setFollowsMap(new Map(followsMap.set(k,v)));
    }
    const {loggedUser, login, logout} = useContext(UserContext);
    const url = `/api/fllws/${loggedUser}`

    useEffect(() => {

      const fetchMsgs = async (f) => {
        setLoading(true);
        fetch("/api/msgs", {
            method: 'GET'
          }).then((response) => {
            return response.json();
          })
            .then((data) => {
              const finalMsgs = data.map((msg) => {
                const copy = {...msg};
                const follow = f.find(follower => follower.name === copy.user);

                if(follow != null) {
                    copy.follow = follow.name;
                }
                return copy

              });

              setMsgs(finalMsgs);

              setLoading(false);
            })
            .catch((error) => console.log(error));
          }

          const fetchFollows = async () => {
            try {
              const response = await fetch(url, { method: 'GET' });
              const data = await response.json();
              const follows = data.follows.map(name => ({ name }));
              const followers = follows.map((user) => {
                updateFollowsMap(user.name, true);
              })
              return follows;
            } catch (error) {
              console.log(error);
              return [];
            }
          }

      fetchFollows()
        .then((f) => fetchMsgs(f))
        .catch(error => console.error('Failed to fetch follows'));
      }, []);

    const handleFollow = async (user) => {
      let body = {follow: user, unfollow: null };

      if (followsMap.get(user)) {
        body = {follow: null, unfollow: user };
      }

      var url = `/api/fllws/${loggedUser}`
      const response = await fetch(url ,
      {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: "POST",
            body: JSON.stringify(body)
        });

        if (response.ok) {
            updateFollowsMap(user, !followsMap.get(user))
        } else {
            const errorData = await response.json();
        }

    }

    if(loading) return (
      <div className="containerBox">
        <div className="centered">
          <Oval 
            height={80}
            width={80}
            color="#f1faee"
          />
        </div>
      </div>

    );

    return (
      <>
      <div className="containerBox">
      <div className="centered">
        <h1 className="timeline-header">{loggedUser}</h1>
        {msgs.map((item) => (
          <div key={item.pub_date} className="post-container">
            <div className="post-header">
              <CgProfile className="icon" size={70}/>
              <p className="post-user">{item.user}</p>
              <div className="post-buttons">
                <button className="follow-button" onClick={() => handleFollow(item.user)}>
                  { followsMap.get(item.user) ? 'Unfollow' : 'Follow'}
                </button>
              </div>
            </div>
            <div className="post-content">
              <div className="post-box">
                <p className="post-text">{item.content}</p>
              </div>
              <br/>
            </div>
          </div>
        ))}
      </div>
    </div>
    </>
    );
}

export default Timeline;
