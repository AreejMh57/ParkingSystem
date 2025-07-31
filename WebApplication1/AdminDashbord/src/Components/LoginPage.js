// src/components/LoginPage.js
import React, { useState } from 'react';
import axios from 'axios'; // ������� ����� axios

function LoginPage() {
    // ����� ����� (states) ������ ������ ������ ������� ������ ����� ����� �������
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [loading, setLoading] = useState(false);

    // ���� ������� ����� �������
    const handleSubmit = async (e) => {
        e.preventDefault(); // ��� ������ ��������� ������� (����� ������)
        setLoading(true); // ����� ���� ������� (���� ���� ����� �����)
        setErrorMessage(''); // ��� �� ����� ��� �����

        try {
            // *** ��� ��� ����� ��� ����� ������ ��� ���Backend ***
            // *** ������ XXXX ���� ������ ������ ���Backend ����� �� (�����: 7169) ***
            const response = await axios.post('https://0.0.0.0:5001/api/Auth/login', {
                email, // ������ ���������� �� ���� ������
                password, // ���� ������ �� ���� ������
                rememberMe: false, // ���� ������� ��� true �� ����� �� ���� ������ �� �������
            }, {
                headers: {
                    'Content-Type': 'application/json', // ��� ������� ���� �����
                },
            });

            // ������ �������� �� �� ������� (token �������� ��������)
            const { token, userInfo } = response.data; // ����� ��� LoginResponseDto

            // *** ����� ������ �� Local Storage ������� (��� ���� �������� �������) ***
            localStorage.setItem('jwt_token', token);

            // ����� ������� �������� (���� �� ���� ����� ���� ��� �������� �� Dashboard)
            localStorage.setItem('user_info', JSON.stringify(userInfo));

            // ��� ����� ���� ������� (������ ������ ������� Routing)
            alert('Login successful! Welcome to the dashboard.');
            window.location.reload(); // ����� ����� ������ (������) ��������

        } catch (error) {
            // ������ ������� �� ��� ���API
            if (error.response) {
                // ��� ��� ����� ������ �� ������� (����� 401 Unauthorized� 400 Bad Request)
                setErrorMessage(error.response.data.message || 'Invalid login credentials. Please try again.');
            } else {
                // ��� �� ������ (������� ��� ���� ����� �����)
                setErrorMessage('Network error. Please ensure the backend is running and reachable.');
            }
            console.error('Login error:', error); // ����� ����� �� console ������� �������� �� �������
        } finally {
            setLoading(false); // ����� ���� ������� ��� ����� �� �������
        }
    };

    // ���� ����� �������� (UI) ����� ����� ������
    return (
        <div style={{ padding: '20px', maxWidth: '400px', margin: '50px auto', border: '1px solid #ccc', borderRadius: '8px', boxShadow: '2px 2px 8px rgba(0,0,0,0.1)' }}>
            <h2 style={{ textAlign: 'center', color: '#333' }}>Login to Admin Dashboard</h2>
            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: '15px' }}>
                    <label htmlFor="email" style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Email:</label>
                    <input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required // ��� �����
                        style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '4px', boxSizing: 'border-box' }}
                    />
                </div>
                <div style={{ marginBottom: '15px' }}>
                    <label htmlFor="password" style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Password:</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required // ��� �����
                        style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '4px', boxSizing: 'border-box' }}
                    />
                </div>
                {errorMessage && <p style={{ color: 'red', marginBottom: '15px', textAlign: 'center' }}>{errorMessage}</p>}
                <button
                    type="submit"
                    disabled={loading} // ����� ���� ����� �������
                    style={{
                        width: '100%',
                        padding: '10px 15px',
                        backgroundColor: '#007bff',
                        color: 'white',
                        border: 'none',
                        borderRadius: '5px',
                        cursor: 'pointer',
                        fontSize: '16px',
                        fontWeight: 'bold',
                        opacity: loading ? 0.7 : 1, // ����� �������� ����� �������
                    }}
                >
                    {loading ? 'Logging in...' : 'Login'}
                </button>
            </form>
        </div>
    );
}

export default LoginPage;