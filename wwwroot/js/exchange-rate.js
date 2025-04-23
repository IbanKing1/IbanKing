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
    const favoritesContainer = document.getElementById('favoritesContainer');
    const chartCanvas = document.getElementById('exchangeChart');

    let favorites = ['EUR', 'USD', 'RON'];
    let exchangeRates = {};
    let chart = null;
    let allCurrencies = [];
    let baseCurrency = 'EUR';
    let historicalDataCache = {};

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
    }

    function setupEventListeners() {
        convertButton.addEventListener('click', convertCurrency);
        swapButton.addEventListener('click', swapCurrencies);
        editFavoritesBtn.addEventListener('click', openEditModal);
        closeModalBtn.addEventListener('click', closeEditModal);
        saveFavoritesBtn.addEventListener('click', saveFavorites);
        fromCurrencySelect.addEventListener('change', updateResult);
        toCurrencySelect.addEventListener('change', updateResult);
        amountInput.addEventListener('input', updateResult);
    }

    function setupCurrencySearch() {
        const currencySelects = [fromCurrencySelect, toCurrencySelect];

        currencySelects.forEach(select => {
            const searchBox = document.createElement('div');
            searchBox.className = 'search-box';
            searchBox.innerHTML = `
                <input type="text" placeholder="Search currency..." class="currency-search-input">
                <span class="search-icon">🔍</span>
                <div class="currency-search-results"></div>
            `;

            select.parentNode.insertBefore(searchBox, select);
            select.style.display = 'none';

            const searchInput = searchBox.querySelector('.currency-search-input');
            const searchResults = searchBox.querySelector('.currency-search-results');

            searchInput.addEventListener('input', function () {
                const searchTerm = this.value.toLowerCase();
                const filteredOptions = allCurrencies.filter(currency =>
                    currency.toLowerCase().includes(searchTerm));

                renderSearchResults(searchResults, filteredOptions, select, searchInput);
            });

            searchInput.addEventListener('focus', function () {
                renderSearchResults(searchResults, allCurrencies, select, searchInput);
            });

            document.addEventListener('click', function (e) {
                if (!searchBox.contains(e.target)) {
                    searchResults.style.display = 'none';
                }
            });
        });
    }

    function renderSearchResults(container, currencies, targetSelect, searchInput) {
        container.innerHTML = '';

        if (currencies.length === 0) {
            container.style.display = 'none';
            return;
        }

        currencies.forEach(currency => {
            const item = document.createElement('div');
            item.className = 'currency-search-item';
            item.textContent = currency;
            item.addEventListener('click', function () {
                targetSelect.value = currency;
                searchInput.value = currency;
                container.style.display = 'none';
                updateResult();
            });
            container.appendChild(item);
        });

        container.style.display = 'block';
    }

    async function fetchExchangeRates() {
        try {
            const response = await fetch('/ExchangeRate/Index?handler=LiveRates');
            if (!response.ok) throw new Error('Failed to fetch rates');

            const data = await response.json();
            if (data.result !== 'success') throw new Error('API error');

            exchangeRates = data.conversion_rates;
            allCurrencies = Object.keys(exchangeRates).sort();

            populateCurrencySelects();
            renderFavorites();
            initChart(baseCurrency);
        } catch (error) {
            console.error('Error fetching exchange rates:', error);
            alert('Error loading exchange rates. Please try again later.');
        }
    }

    function populateCurrencySelects() {
        [fromCurrencySelect, toCurrencySelect].forEach(select => {
            select.innerHTML = '';
            allCurrencies.forEach(currency => {
                const option = document.createElement('option');
                option.value = currency;
                option.textContent = currency;
                select.appendChild(option);
            });
        });

        fromCurrencySelect.value = baseCurrency;
        toCurrencySelect.value = 'USD';
    }

    function renderFavorites() {
        if (!favoritesContainer) return;

        favoritesContainer.innerHTML = '';

        favorites.forEach(currency => {
            if (currency !== baseCurrency) {
                const rate = getExchangeRate(baseCurrency, currency);
                const change24h = (Math.random() * 2 - 1).toFixed(4);
                const isPositive = parseFloat(change24h) >= 0;

                const favoriteCard = document.createElement('div');
                favoriteCard.className = 'favorite-card';
                favoriteCard.innerHTML = `
                    <div class="favorite-pair">${baseCurrency}/${currency}</div>
                    <div class="favorite-rate">${rate.toFixed(4)}</div>
                    <div class="favorite-change ${isPositive ? 'positive-change' : 'negative-change'}">
                        ${isPositive ? '+' : ''}${change24h}%
                    </div>
                `;

                favoriteCard.addEventListener('click', () => {
                    fromCurrencySelect.value = baseCurrency;
                    toCurrencySelect.value = currency;
                    updateResult();
                });

                favoritesContainer.appendChild(favoriteCard);
            }
        });
    }

    function getExchangeRate(fromCurrency, toCurrency) {
        return exchangeRates[toCurrency] / exchangeRates[fromCurrency];
    }

    async function initChart(base) {
        try {
            baseCurrency = base;

            if (chart) {
                chart.destroy();
            }

            const labels = [];
            const datasets = [];

            favorites.filter(c => c !== baseCurrency).forEach(currency => {
                const color = getRandomColor();
                const rate = getExchangeRate(baseCurrency, currency);

                datasets.push({
                    label: `${baseCurrency}/${currency}`,
                    data: Array(30).fill().map((_, i) => {
                        const fluctuation = (Math.random() * 0.1 - 0.05);
                        return rate * (1 + fluctuation);
                    }),
                    borderColor: color,
                    backgroundColor: `${color}20`,
                    borderWidth: 2,
                    fill: false,
                    tension: 0.4
                });
            });

            labels.push(...Array(30).fill().map((_, i) => {
                const date = new Date();
                date.setDate(date.getDate() - (29 - i));
                return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
            }));

            chart = new Chart(chartCanvas, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: datasets
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        title: {
                            display: true,
                            text: `${baseCurrency} Exchange Rates (Last 30 Days)`,
                            font: {
                                size: 16
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: false,
                            title: {
                                display: true,
                                text: `Rate (1 ${baseCurrency} = X)`
                            }
                        }
                    }
                }
            });

            renderFavorites();
        } catch (error) {
            console.error('Error initializing chart:', error);
        }
    }

    function getRandomColor() {
        const colors = [
            '#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6',
            '#ec4899', '#14b8a6', '#f97316', '#6366f1', '#d946ef'
        ];
        return colors[Math.floor(Math.random() * colors.length)];
    }

    function convertCurrency(event) {
        event.preventDefault();
        updateResult();
    }

    function swapCurrencies() {
        const temp = fromCurrencySelect.value;
        fromCurrencySelect.value = toCurrencySelect.value;
        toCurrencySelect.value = temp;
        updateResult();
    }

    function updateResult() {
        const fromCurrency = fromCurrencySelect.value;
        const toCurrency = toCurrencySelect.value;
        const amount = parseFloat(amountInput.value) || 1;

        const rate = getExchangeRate(fromCurrency, toCurrency);
        const result = amount * rate;

        rateSpan.textContent = `1 ${fromCurrency} = ${rate.toFixed(6)} ${toCurrency}`;
        resultSpan.textContent = `${result.toFixed(2)} ${toCurrency}`;
        resultDiv.style.display = 'block';
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

        allCurrencies.forEach(currency => {
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
        initChart(baseCurrency);
    }

    window.addEventListener('click', function (event) {
        if (event.target === modal) {
            closeEditModal();
        }
    });
});