// src/App.js
import React from 'react';
import './App.css'; // ���� CSS ������� ����� �������� �� �� ������
import LoginPage from './components/LoginPage'; // ������� ���� LoginPage

function App() {
    // ������ ����� ��� ���� ����� ������.
    // ������ ����� ����� ���� ��� ������ ��� ��� ��� �������� ������ ������ (�������� ������ �� localStorage)
    // ������ ���� ������ �������� ����� �� ���� ������.
    return (
        <div className="App">
            <LoginPage /> {/* ������� ���� LoginPage */}
        </div>
    );
}

export default App;