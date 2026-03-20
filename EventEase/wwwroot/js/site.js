// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Make table rows clickable
document.addEventListener('DOMContentLoaded', function () {
    // Add click event to table rows with data-url attribute or that are clickable
    const clickableRows = document.querySelectorAll('tr[data-href], tr.clickable');

    clickableRows.forEach(row => {
        row.addEventListener('click', function () {
            const href = this.dataset.href;
            if (href) {
                window.location.href = href;
            }
        });
        row.style.cursor = 'pointer';
    });

    // Add hover effect to cards
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-5px)';
        });
        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.remove();
            }, 500);
        }, 5000);
    });
});