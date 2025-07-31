// src/components/LoginPage.js
import React, { useState } from 'react';
import axios from 'axios'; // ÇÓÊíÑÇÏ ãßÊÈÉ axios

function LoginPage() {
    // ÊÚÑíİ ÍÇáÇÊ (states) Çáãßæä áÅÏÇÑÉ ÈíÇäÇÊ ÇáäãæĞÌ æÑÓÇÆá ÇáÎØÃ æÍÇáÉ ÇáÊÍãíá
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [loading, setLoading] = useState(false);

    // ÏÇáÉ áãÚÇáÌÉ ÅÑÓÇá ÇáäãæĞÌ
    const handleSubmit = async (e) => {
        e.preventDefault(); // ãäÚ ÇáÓáæß ÇáÇİÊÑÇÖí ááãÊÕİÍ (ÊÍÏíË ÇáÕİÍÉ)
        setLoading(true); // ÊİÚíá ÍÇáÉ ÇáÊÍãíá (áÚÑÖ ãÄÔÑ ÊÍãíá ãËáÇğ)
        setErrorMessage(''); // ãÓÍ Ãí ÑÓÇÆá ÎØÃ ÓÇÈŞÉ

        try {
            // *** åäÇ íÊã ÅÑÓÇá ØáÈ ÊÓÌíá ÇáÏÎæá Åáì ÇáÜBackend ***
            // *** ÇÓÊÈÏá XXXX ÈÑŞã ÇáÈæÑÊ ÇáİÚáí ááÜBackend ÇáÎÇÕ Èß (ãËáÇğ: 7169) ***
            const response = await axios.post('https://0.0.0.0:5001/api/Auth/login', {
                email, // ÇáÈÑíÏ ÇáÅáßÊÑæäí ãä ÍÇáÉ Çáãßæä
                password, // ßáãÉ ÇáãÑæÑ ãä ÍÇáÉ Çáãßæä
                rememberMe: false, // íãßä ÊÚííäåÇ Åáì true Ãæ ÌáÈåÇ ãä ãÑÈÚ ÇÎÊíÇÑ İí ÇáæÇÌåÉ
            }, {
                headers: {
                    'Content-Type': 'application/json', // äæÚ ÇáãÍÊæì ÇáĞí äÑÓáå
                },
            });

            // ÇÓÊáÇã ÇáÈíÇäÇÊ ãä ÑÏ ÇáÓíÑİÑ (token æãÚáæãÇÊ ÇáãÓÊÎÏã)
            const { token, userInfo } = response.data; // ÈäÇÁğ Úáì LoginResponseDto

            // *** ÊÎÒíä ÇáÊæßä İí Local Storage ááãÊÕİÍ (ãåã ÌÏÇğ ááãÕÇÏŞÉ ÇááÇÍŞÉ) ***
            localStorage.setItem('jwt_token', token);

            // ÊÎÒíä ãÚáæãÇÊ ÇáãÓÊÎÏã (íãßä Ãä Êßæä ãİíÏÉ áÚÑÖ ÇÓã ÇáãÓÊÎÏã İí Dashboard)
            localStorage.setItem('user_info', JSON.stringify(userInfo));

            // ÚÑÖ ÑÓÇáÉ äÌÇÍ æÇäÊŞÇá (ãÄŞÊÇğ¡ áÇÍŞÇğ ÓäÓÊÎÏã Routing)
            alert('Login successful! Welcome to the dashboard.');
            window.location.reload(); // ÅÚÇÏÉ ÊÍãíá ÇáÕİÍÉ (ãÄŞÊÇğ) ááÇäÊŞÇá

        } catch (error) {
            // ãÚÇáÌÉ ÇáÃÎØÇÁ ãä ØáÈ ÇáÜAPI
            if (error.response) {
                // ÅĞÇ ßÇä ÇáÎØÃ ŞÇÏãÇğ ãä ÇáÓíÑİÑ (ãËáÇğ 401 Unauthorized¡ 400 Bad Request)
                setErrorMessage(error.response.data.message || 'Invalid login credentials. Please try again.');
            } else {
                // ÎØÃ İí ÇáÔÈßÉ (ÇáÓíÑİÑ ÛíÑ ÔÛÇá¡ ãÔßáÉ ÇÊÕÇá)
                setErrorMessage('Network error. Please ensure the backend is running and reachable.');
            }
            console.error('Login error:', error); // ØÈÇÚÉ ÇáÎØÃ İí console ÇáãÊÕİÍ ááãÓÇÚÏÉ İí ÇáÊÕÍíÍ
        } finally {
            setLoading(false); // ÅíŞÇİ ÍÇáÉ ÇáÊÍãíá ÈÛÖ ÇáäÙÑ Úä ÇáäÊíÌÉ
        }
    };

    // åíßá æÇÌåÉ ÇáãÓÊÎÏã (UI) áÕİÍÉ ÊÓÌíá ÇáÏÎæá
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
                        required // ÍŞá ãØáæÈ
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
                        required // ÍŞá ãØáæÈ
                        style={{ width: '100%', padding: '10px', border: '1px solid #ddd', borderRadius: '4px', boxSizing: 'border-box' }}
                    />
                </div>
                {errorMessage && <p style={{ color: 'red', marginBottom: '15px', textAlign: 'center' }}>{errorMessage}</p>}
                <button
                    type="submit"
                    disabled={loading} // ÊÚØíá ÇáÒÑ ÃËäÇÁ ÇáÊÍãíá
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
                        opacity: loading ? 0.7 : 1, // ÊŞáíá ÇáÔİÇİíÉ ÃËäÇÁ ÇáÊÍãíá
                    }}
                >
                    {loading ? 'Logging in...' : 'Login'}
                </button>
            </form>
        </div>
    );
}

export default LoginPage;