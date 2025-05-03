function copyIBAN(iban) {
    navigator.clipboard.writeText(iban).then(function () {
        alert("IBAN copied to clipboard: " + iban);
    });
}

let balanceVisible = false;

function toggleBalanceVisibility() {
    const toggleButton = document.getElementById("toggleBalanceBtn");
    if (toggleButton) toggleButton.blur();

    balanceVisible = !balanceVisible;
    const balances = document.querySelectorAll('.account-balance');
    const eyeIcon = document.getElementById("eyeIcon");

    balances.forEach(el => {
        el.textContent = balanceVisible ? el.dataset.realvalue : '••••••';
    });

    if (balanceVisible) {
        eyeIcon.outerHTML = `
            <svg id="eyeIcon" xmlns="http://www.w3.org/2000/svg"
                 width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
                <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8z"/>
                <path d="M8 5a3 3 0 1 1 0 6a3 3 0 0 1 0-6z"/>
            </svg>`;
    } else {
        eyeIcon.outerHTML = `
            <svg id="eyeIcon" xmlns="http://www.w3.org/2000/svg"
                 width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
                <path d="M13.359 11.238a9.469 9.469 0 0 0 2.273-3.238
                         s-3-5.5-8-5.5a9.719 9.719 0 0 0-3.366.66L5.43 4.43
                         a6.5 6.5 0 0 1 6.218 1.082 3 3 0 0 1-4.087 4.087
                         6.5 6.5 0 0 1-1.082-6.218L2.293 1.707a1 1 0 0 0-1.414 1.414
                         l12 12a1 1 0 0 0 1.414-1.414l-1.934-1.934z"/>
            </svg>`;
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const balances = document.querySelectorAll('.account-balance');
    balances.forEach(el => {
        el.dataset.realvalue = el.textContent.trim();
        el.textContent = '••••••';
    });

    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
