// src/App.js
import React from 'react';
import './App.css'; // בדבÝ CSS ÇבÑÆםÓם¡ םד‗ה‗ ÇבÇÍÊÝÇÙ Èו Ãז ÅÒÇבÊו
import LoginPage from './components/LoginPage'; // ÇÓÊםÑÇÏ ד‗זה LoginPage

function App() {
    // ÍÇבםÇנ¡ ÓהÚÑÖ ÝÞØ ÕÝÍÉ ÊÓÌםב ÇבÏÎזב.
    // בÇÍÞÇנ¡ םד‗ה‗ ÅÖÇÝÉ דהØÞ והÇ בבÊÍÞÞ דדÇ ÅÐÇ ‗Çה ÇבדÓÊÎÏד דÓÌבÇנ בבÏÎזב (ÈÇÓÊÎÏÇד ÇבÊז‗ה Ýם localStorage)
    // זÅÙוÇÑ בזÍÉ ÇבÊÍ‗ד ÇבÑÆםÓםÉ ÈÏבÇנ דה ÕÝÍÉ ÇבÏÎזב.
    return (
        <div className="App">
            <LoginPage /> {/* ÇÓÊÎÏÇד ד‗זה LoginPage */}
        </div>
    );
}

export default App;