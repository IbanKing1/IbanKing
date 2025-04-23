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
    let currentFromCurrency = 'RON';
    let currentToCurrency = 'EUR';

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
        fromCurrencySelect.addEventListener('change', updateFromCurrency);
        toCurrencySelect.addEventListener('change', updateToCurrency);
        amountInput.addEventListener('input', updateResult);
    }

    function setupCurrencySearch() {
        const currencySelects = [fromCurrencySelect, toCurrencySelect];
        const searchInputs = [];

        currencySelects.forEach((select, index) => {
            const searchBox = document.createElement('div');
            searchBox.className = 'search-box';
            searchBox.innerHTML = `
                <input type="text" placeholder="Search currency..." class="currency-search-input" 
                       value="${index === 0 ? 'RON' : 'EUR'}">
                <span class="search-icon">🔍</span>
                <div class="currency-search-results"></div>
            `;

            select.parentNode.insertBefore(searchBox, select);
            select.style.display = 'none';

            const searchInput = searchBox.querySelector('.currency-search-input');
            const searchResults = searchBox.querySelector('.currency-search-results');
            searchInputs.push(searchInput);

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

        return searchInputs;
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
                if (targetSelect === fromCurrencySelect) {
                    currentFromCurrency = currency;
                } else {
                    currentToCurrency = currency;
                }
                updateResult();
                fetchHistoricalData();
            });
            container.appendChild(item);
        });

        container.style.display = 'block';
    }

    async function fetchExchangeRates() {
        try {
            const response = await fetch('https://api.frankfurter.app/latest');
            if (!response.ok) throw new Error('Failed to fetch rates');

            const data = await response.json();
            exchangeRates = data.rates;
            exchangeRates[data.base] = 1;
            allCurrencies = Object.keys(exchangeRates).sort();

            populateCurrencySelects();
            renderFavorites();
            fetchHistoricalData();
        } catch (error) {
            console.error('Error fetching exchange rates:', error);
            alert('Error loading exchange rates. Please try again later.');
        }
    }

    async function fetchHistoricalData() {
        try {
            const endDate = new Date();
            const startDate = new Date();
            startDate.setDate(endDate.getDate() - 30);

            const response = await fetch(
                `https://api.frankfurter.app/${formatDate(startDate)}..${formatDate(endDate)}?from=${currentFromCurrency}&to=${currentToCurrency}`
            );
            if (!response.ok) throw new Error('Failed to fetch historical data');

            const data = await response.json();
            renderChart(data);
        } catch (error) {
            console.error('Error fetching historical data:', error);
        }
    }

    function formatDate(date) {
        return date.toISOString().split('T')[0];
    }

    function renderChart(data) {
        if (!data.rates) return;

        const labels = Object.keys(data.rates).map(date => {
            const d = new Date(date);
            return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
        });

        const values = Object.values(data.rates).map(rate => rate[currentToCurrency]);

        if (chart) {
            chart.destroy();
        }

        chart = new Chart(chartCanvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: `${currentFromCurrency}/${currentToCurrency}`,
                    data: values,
                    borderColor: '#3b82f6',
                    backgroundColor: '#3b82f620',
                    borderWidth: 2,
                    fill: false,
                    tension: 0.1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: `${currentFromCurrency}/${currentToCurrency} Exchange Rate (Last 30 Days)`,
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
                            text: `Rate (1 ${currentFromCurrency} = X ${currentToCurrency})`
                        }
                    }
                }
            }
        });
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

        fromCurrencySelect.value = currentFromCurrency;
        toCurrencySelect.value = currentToCurrency;
    }

    function renderFavorites() {
        if (!favoritesContainer) return;

        favoritesContainer.innerHTML = '';

        favorites.forEach(currency => {
            if (currency !== currentFromCurrency) {
                const rate = getExchangeRate(currentFromCurrency, currency);
                const favoriteCard = document.createElement('div');
                favoriteCard.className = 'favorite-card';
                favoriteCard.innerHTML = `
                    <div class="favorite-pair">${currentFromCurrency}/${currency}</div>
                    <div class="favorite-rate">${rate.toFixed(4)}</div>
                `;

                favoriteCard.addEventListener('click', () => {
                    const fromSearch = document.querySelector('.currency-select-wrapper:first-child .currency-search-input');
                    const toSearch = document.querySelector('.currency-select-wrapper:last-child .currency-search-input');

                    fromCurrencySelect.value = currentFromCurrency;
                    toCurrencySelect.value = currency;
                    currentToCurrency = currency;

                    if (fromSearch) fromSearch.value = currentFromCurrency;
                    if (toSearch) toSearch.value = currentToCurrency;

                    updateResult();
                    fetchHistoricalData();
                });

                favoritesContainer.appendChild(favoriteCard);
            }
        });
    }

    function getExchangeRate(fromCurrency, toCurrency) {
        return exchangeRates[toCurrency] / exchangeRates[fromCurrency];
    }

    function convertCurrency(event) {
        event.preventDefault();
        updateResult();
    }

    function swapCurrencies() {
        const temp = currentFromCurrency;
        currentFromCurrency = currentToCurrency;
        currentToCurrency = temp;

        fromCurrencySelect.value = currentFromCurrency;
        toCurrencySelect.value = currentToCurrency;

        const fromSearch = document.querySelector('.currency-select-wrapper:first-child .currency-search-input');
        const toSearch = document.querySelector('.currency-select-wrapper:last-child .currency-search-input');

        if (fromSearch) fromSearch.value = currentFromCurrency;
        if (toSearch) toSearch.value = currentToCurrency;

        updateResult();
        fetchHistoricalData();
        renderFavorites();
    }

    function updateFromCurrency() {
        currentFromCurrency = fromCurrencySelect.value;
        const searchInput = document.querySelector('.currency-select-wrapper:first-child .currency-search-input');
        if (searchInput) searchInput.value = currentFromCurrency;
        updateResult();
        fetchHistoricalData();
        renderFavorites();
    }

    function updateToCurrency() {
        currentToCurrency = toCurrencySelect.value;
        const searchInput = document.querySelector('.currency-select-wrapper:last-child .currency-search-input');
        if (searchInput) searchInput.value = currentToCurrency;
        updateResult();
        fetchHistoricalData();
    }

    function updateResult() {
        const amount = parseFloat(amountInput.value) || 1;
        const rate = getExchangeRate(currentFromCurrency, currentToCurrency);
        const result = amount * rate;

        rateSpan.textContent = `1 ${currentFromCurrency} = ${rate.toFixed(6)} ${currentToCurrency}`;
        resultSpan.textContent = `${result.toFixed(2)} ${currentToCurrency}`;
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
    }

    window.addEventListener('click', function (event) {
        if (event.target === modal) {
            closeEditModal();
        }
    });
});