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
    let allCurrencies = [];

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
        fromCurrencySelect.addEventListener('change', updateChart);
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

                renderSearchResults(searchResults, filteredOptions, select);
            });

            searchInput.addEventListener('focus', function () {
                renderSearchResults(searchResults, allCurrencies, select);
            });

            document.addEventListener('click', function (e) {
                if (!searchBox.contains(e.target)) {
                    searchResults.style.display = 'none';
                }
            });
        });
    }

    function renderSearchResults(container, currencies, targetSelect) {
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
                container.style.display = 'none';
                if (targetSelect === fromCurrencySelect) {
                    updateChart();
                }
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
            exchangeRates = data.rates;
            allCurrencies = Object.keys(exchangeRates);

            populateCurrencySelects();
            renderFavorites();
            initChart('EUR', 'USD');
        } catch (error) {
            console.error('Error fetching exchange rates:', error);
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

            if (select === fromCurrencySelect) {
                select.value = 'EUR';
            } else {
                select.value = 'USD';
            }
        });
    }

    function renderFavorites() {
        if (!favoritesContainer) return;

        favoritesContainer.innerHTML = '';

        for (let i = 0; i < favorites.length; i++) {
            for (let j = 0; j < favorites.length; j++) {
                if (i !== j) {
                    const fromCurrency = favorites[i];
                    const toCurrency = favorites[j];
                    const rate = getExchangeRate(fromCurrency, toCurrency);

                    const favoriteCard = document.createElement('div');
                    favoriteCard.className = 'favorite-card';
                    favoriteCard.innerHTML = `
                        <div class="favorite-pair">${fromCurrency}/${toCurrency}</div>
                        <div class="favorite-rate">${rate ? rate.toFixed(4) : '--'}</div>
                    `;

                    favoriteCard.addEventListener('click', () => {
                        fromCurrencySelect.value = fromCurrency;
                        toCurrencySelect.value = toCurrency;
                        updateChart();
                    });

                    favoritesContainer.appendChild(favoriteCard);
                }
            }
        }
    }

    function getExchangeRate(fromCurrency, toCurrency) {
        if (!exchangeRates[fromCurrency] || !exchangeRates[toCurrency]) return null;
        return exchangeRates[toCurrency] / exchangeRates[fromCurrency];
    }

    async function initChart(fromCurrency, toCurrency) {
        try {
            const historicalData = await fetchHistoricalData(fromCurrency, toCurrency);

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
        } catch (error) {
            console.error('Error initializing chart:', error);
        }
    }

    async function fetchHistoricalData(fromCurrency, toCurrency) {
        try {
            const response = await fetch(`/ExchangeRate/Index?handler=HistoricalData&fromCurrency=${fromCurrency}&toCurrency=${toCurrency}`);
            if (!response.ok) throw new Error('Failed to fetch historical data');

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching historical data:', error);
            return generateFallbackData(fromCurrency, toCurrency);
        }
    }

    function generateFallbackData(fromCurrency, toCurrency) {
        const labels = [];
        const values = [];
        const baseRate = getExchangeRate(fromCurrency, toCurrency) || 1.2;

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
            const rate = getExchangeRate(fromCurrency, toCurrency);
            const result = amount * rate;

            rateSpan.textContent = `1 ${fromCurrency} = ${rate.toFixed(6)} ${toCurrency}`;
            resultSpan.textContent = `${result.toFixed(2)} ${toCurrency}`;
            resultDiv.style.display = 'block';

            updateChart();
        } catch (error) {
            console.error('Error converting currency:', error);
            alert('Error converting currency. Please try again.');
        }
    }

    function swapCurrencies() {
        const temp = fromCurrencySelect.value;
        fromCurrencySelect.value = toCurrencySelect.value;
        toCurrencySelect.value = temp;
        updateChart();
    }

    function updateChart() {
        const fromCurrency = fromCurrencySelect.value;
        const toCurrency = toCurrencySelect.value;
        initChart(fromCurrency, toCurrency);
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
    }

    window.addEventListener('click', function (event) {
        if (event.target === modal) {
            closeEditModal();
        }
    });
});