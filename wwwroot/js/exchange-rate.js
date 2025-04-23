document.addEventListener('DOMContentLoaded', function () {
    const fromCurrencySelect = document.getElementById('fromCurrency');
    const toCurrencySelect = document.getElementById('toCurrency');
    const amountInput = document.getElementById('amount');
    const swapButton = document.getElementById('swapCurrencies');
    const convertButton = document.getElementById('convertButton');
    const resultDiv = document.getElementById('conversionResult');
    const rateSpan = document.getElementById('rate');
    const resultSpan = document.getElementById('result');
    const editFavoritesBtn = document.getElementById('editFavorites');
    const modal = document.getElementById('editModal');
    const closeModalBtn = document.getElementById('closeModal');
    const saveFavoritesBtn = document.getElementById('saveFavorites');
    const currencySearch = document.getElementById('currencySearch');
    const favoritesContainer = document.getElementById('favoritesContainer');
    const chartCanvas = document.getElementById('exchangeChart');

    let favorites = ['EUR', 'USD', 'RON'];
    let exchangeRates = {};
    let chart = null;

    init();

    function init() {
        loadFavorites();
        setupEventListeners();
        fetchExchangeRates();
        setupCurrencySearch();
    }

    function loadFavorites() {
        const savedFavorites = localStorage.getItem('currencyFavorites');
        if (savedFavorites) {
            favorites = JSON.parse(savedFavorites);
        }
        renderFavorites();
    }

    function setupEventListeners() {
        convertButton.addEventListener('click', convertCurrency);
        swapButton.addEventListener('click', swapCurrencies);

        editFavoritesBtn.addEventListener('click', openEditModal);
        closeModalBtn.addEventListener('click', closeEditModal);
        saveFavoritesBtn.addEventListener('click', saveFavorites);

        fromCurrencySelect.addEventListener('change', function () {
            updateToCurrencyOptions();
        });
    }

    function setupCurrencySearch() {
        const currencySelects = [fromCurrencySelect, toCurrencySelect];

        currencySelects.forEach(select => {
            const originalOptions = Array.from(select.options);

            select.addEventListener('focus', function () {
                this.size = 5; 
            });

            select.addEventListener('blur', function () {
                this.size = 1;
            });

            const searchBox = document.createElement('div');
            searchBox.className = 'search-box';
            searchBox.innerHTML = `
                <input type="text" placeholder="Search currency..." class="currency-search">
                <span class="search-icon">🔍</span>
            `;

            select.parentNode.insertBefore(searchBox, select);

            const searchInput = searchBox.querySelector('.currency-search');

            searchInput.addEventListener('input', function () {
                const searchTerm = this.value.toLowerCase();
                const filteredOptions = originalOptions.filter(option =>
                    option.text.toLowerCase().includes(searchTerm) ||
                    option.value.toLowerCase().includes(searchTerm));

                select.innerHTML = '';
                filteredOptions.forEach(option => {
                    select.appendChild(option.cloneNode(true));
                });
            });
        });
    }

    async function fetchExchangeRates() {
        try {
            const response = await fetch('/ExchangeRate/Index?handler=LiveRates');
            if (!response.ok) throw new Error('Failed to fetch rates');

            const data = await response.json();
            exchangeRates = data;

            renderFavorites();

            if (chartCanvas) {
                initChart('EUR', 'USD');
            }
        } catch (error) {
            console.error('Error fetching exchange rates:', error);
        }
    }

    function renderFavorites() {
        if (!favoritesContainer) return;

        favoritesContainer.innerHTML = '';

        for (let i = 0; i < favorites.length; i++) {
            for (let j = 0; j < favorites.length; j++) {
                if (i !== j) {
                    const fromCurrency = favorites[i];
                    const toCurrency = favorites[j];

                    const rate = exchangeRates[`${fromCurrency}_${toCurrency}`] ||
                        (1 / (exchangeRates[`${toCurrency}_${fromCurrency}`] || 1));

                    const change = (Math.random() * 2 - 1).toFixed(4);
                    const isPositive = parseFloat(change) >= 0;

                    const favoriteCard = document.createElement('div');
                    favoriteCard.className = 'favorite-card';
                    favoriteCard.innerHTML = `
                        <div class="favorite-pair">${fromCurrency}/${toCurrency}</div>
                        <div class="favorite-rate">${rate ? rate.toFixed(4) : '--'}</div>
                        <div class="favorite-change ${isPositive ? 'positive-change' : 'negative-change'}">
                            ${isPositive ? '+' : ''}${change}%
                        </div>
                    `;

                    favoriteCard.addEventListener('click', () => {
                        fromCurrencySelect.value = fromCurrency;
                        toCurrencySelect.value = toCurrency;
                        updateToCurrencyOptions();
                    });

                    favoritesContainer.appendChild(favoriteCard);
                }
            }
        }
    }

    function initChart(fromCurrency, toCurrency) {
        const historicalData = generateHistoricalData(fromCurrency, toCurrency);

        if (chart) {
            chart.destroy();
        }

        chart = new Chart(chartCanvas, {
            type: 'line',
            data: {
                labels: historicalData.labels,
                datasets: [{
                    label: `${fromCurrency} to ${toCurrency}`,
                    data: historicalData.values,
                    borderColor: '#0056b3',
                    backgroundColor: 'rgba(0, 86, 179, 0.1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: `${fromCurrency}/${toCurrency} Exchange Rate (Last 30 Days)`,
                        font: {
                            size: 16
                        }
                    },
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: false,
                        title: {
                            display: true,
                            text: `Rate (1 ${fromCurrency} = X ${toCurrency})`
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Date'
                        }
                    }
                }
            }
        });
    }

    function generateHistoricalData(fromCurrency, toCurrency) {
        const labels = [];
        const values = [];
        const baseRate = exchangeRates[`${fromCurrency}_${toCurrency}`] || 1.2;

        for (let i = 29; i >= 0; i--) {
            const date = new Date();
            date.setDate(date.getDate() - i);
            labels.push(date.toLocaleDateString());

            const fluctuation = (Math.random() * 0.1 - 0.05);
            values.push(baseRate * (1 + fluctuation));
        }

        return { labels, values };
    }

    async function convertCurrency(event) {
        event.preventDefault();

        const fromCurrency = fromCurrencySelect.value;
        const toCurrency = toCurrencySelect.value;
        const amount = parseFloat(amountInput.value);

        if (!amount || isNaN(amount)) {
            alert('Please enter a valid amount');
            return;
        }

        try {
            const response = await fetch(`/ExchangeRate/Index?handler=Convert&fromCurrency=${fromCurrency}&toCurrency=${toCurrency}&amount=${amount}`);
            if (!response.ok) throw new Error('Conversion failed');

            const data = await response.json();

            rateSpan.textContent = `1 ${fromCurrency} = ${data.conversion_rate.toFixed(6)} ${toCurrency}`;
            resultSpan.textContent = `${data.conversion_result.toFixed(2)} ${toCurrency}`;
            resultDiv.style.display = 'block';

            if (chartCanvas) {
                initChart(fromCurrency, toCurrency);
            }
        } catch (error) {
            console.error('Error converting currency:', error);
            alert('Error converting currency. Please try again.');
        }
    }

    function swapCurrencies() {
        const temp = fromCurrencySelect.value;
        fromCurrencySelect.value = toCurrencySelect.value;
        toCurrencySelect.value = temp;
        updateToCurrencyOptions();
    }

    function updateToCurrencyOptions() {
        if (fromCurrencySelect.value === toCurrencySelect.value) {
            for (let option of toCurrencySelect.options) {
                if (option.value !== fromCurrencySelect.value) {
                    toCurrencySelect.value = option.value;
                    break;
                }
            }
        }
    }

    function openEditModal() {
        modal.style.display = 'flex';
        renderAvailableCurrencies();
    }

    function closeEditModal() {
        modal.style.display = 'none';
    }

    function renderAvailableCurrencies() {
        const container = document.getElementById('availableCurrencies');
        if (!container) return;

        container.innerHTML = '';

        const allCurrencies = new Set();
        for (const key in exchangeRates) {
            const [from, to] = key.split('_');
            allCurrencies.add(from);
            allCurrencies.add(to);
        }

        const sortedCurrencies = Array.from(allCurrencies).sort();

        sortedCurrencies.forEach(currency => {
            const isSelected = favorites.includes(currency);

            const option = document.createElement('label');
            option.className = `currency-option ${isSelected ? 'selected' : ''}`;
            option.innerHTML = `
                <input type="checkbox" ${isSelected ? 'checked' : ''} value="${currency}">
                ${currency}
            `;

            container.appendChild(option);
        });

        document.querySelectorAll('.currency-option input').forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                this.parentElement.classList.toggle('selected', this.checked);
            });
        });
    }

    function saveFavorites() {
        const selectedCurrencies = Array.from(
            document.querySelectorAll('.currency-option input:checked')
        ).map(checkbox => checkbox.value);

        if (selectedCurrencies.length < 2) {
            alert('Please select at least 2 currencies');
            return;
        }

        favorites = selectedCurrencies;
        localStorage.setItem('currencyFavorites', JSON.stringify(favorites));
        renderFavorites();
        closeEditModal();
    }

    window.addEventListener('click', function (event) {
        if (event.target === modal) {
            closeEditModal();
        }
    });
});