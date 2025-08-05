// File: wwwroot/js/dashboard.js
using System;

document.addEventListener("DOMContentLoaded", function() {
    console.log("Dashboard script loaded!");

    // Example: Add a click event to all dashboard cards
    const cards = document.querySelectorAll('.dashboard-card');
    cards.forEach(card => {
    card.addEventListener('click', function() {
        // You can add logic here, e.g., to redirect
        // window.location.href = this.querySelector('a').href;
    });
});
});