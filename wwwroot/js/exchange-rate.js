document.addEventListener('DOMContentLoaded', function () {
    const fromCurrencySearch = document.getElementById('fromCurrencySearch');
    const toCurrencySearch = document.getElementById('toCurrencySearch');
    const fromCurrencyDropdown = document.getElementById('fromCurrencyDropdown');
    const toCurrencyDropdown = document.getElementById('toCurrencyDropdown');
    const amountInput = document.getElementById('amount');
    const swapButton = document.getElementById('swapCurrencies');
    const convertButton = document.getElementById('convertButton');
    const resultDiv = document.getElementById('conversionResult');
    const rateSpan = document.getElementById('rate');
    const resultSpan = document.getElementById('result');
    const toggleEditFavoritesBtn = document.getElementById('toggleEditFavorites');
    const editFavoritesSection = document.getElementById('editFavoritesSection');
    const favoritesContainer = document.getElementById('favoritesContainer');
    const chartCanvas = document.getElementById('exchangeChart');
    const currencySearch = document.getElementById('currencySearch');
    const saveFavoritesBtn = document.getElementById('saveFavorites');
    const chartTitle = document.getElementById('chartTitle');

    let favorites = ['EUR', 'USD', 'GBP', 'RON'];
    let exchangeRates = {};
    let chart = null;
    let allCurrencies = [];
    let baseCurrency = 'RON';
    let fromCurrency = 'RON';
    let toCurrency = 'EUR';
    let resizeTimeout;

    init();

    function init() {
        loadFavorites();
        setupEventListeners();
        fetchExchangeRates();
        setupResizeObserver();
    }

    function loadFavorites() {
        const savedFavorites = localStorage.getItem('currencyFavorites');
        if (savedFavorites) {
            favorites = JSON.parse(savedFavorites);
        }
        const savedBase = localStorage.getItem('baseCurrency');
        if (savedBase) {
            baseCurrency = savedBase;
            fromCurrency = savedBase;
        }
    }

    function setupEventListeners() {
        convertButton.addEventListener('click', convertCurrency);
        swapButton.addEventListener('click', swapCurrencies);
        toggleEditFavoritesBtn.addEventListener('click', toggleEditFavorites);
        saveFavoritesBtn.addEventListener('click', saveFavorites);
        amountInput.addEventListener('input', updateResult);
        currencySearch.addEventListener('input', filterCurrencies);

        fromCurrencySearch.addEventListener('input', () => filterCurrencyOptions('from'));
        fromCurrencySearch.addEventListener('focus', () => showCurrencyDropdown('from', true));
        fromCurrencySearch.addEventListener('blur', () => setTimeout(() => hideCurrencyDropdown('from'), 200));
        fromCurrencySearch.addEventListener('keydown', (e) => handleDropdownKeydown(e, 'from'));

        toCurrencySearch.addEventListener('input', () => filterCurrencyOptions('to'));
        toCurrencySearch.addEventListener('focus', () => showCurrencyDropdown('to', true));
        toCurrencySearch.addEventListener('blur', () => setTimeout(() => hideCurrencyDropdown('to'), 200));
        toCurrencySearch.addEventListener('keydown', (e) => handleDropdownKeydown(e, 'to'));

        document.addEventListener('click', (e) => {
            if (!e.target.closest('.currency-select-wrapper')) {
                hideCurrencyDropdown('from');
                hideCurrencyDropdown('to');
            }
        });
    }

    function setupResizeObserver() {
        const resizeObserver = new ResizeObserver(() => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                if (chart) {
                    chart.resize();
                    chart.update();
                }
            }, 200);
        });
        resizeObserver.observe(chartCanvas);
    }

    function filterCurrencyOptions(type) {
        const searchBox = type === 'from' ? fromCurrencySearch : toCurrencySearch;
        const dropdown = type === 'from' ? fromCurrencyDropdown : toCurrencyDropdown;
        const searchTerm = searchBox.value.toLowerCase();

        const filteredCurrencies = allCurrencies.filter(currency =>
            currency.toLowerCase().includes(searchTerm)
        );

        renderDropdownOptions(dropdown, filteredCurrencies, type);

        if (filteredCurrencies.length > 0) {
            const firstOption = dropdown.querySelector('.currency-dropdown-option');
            if (firstOption) {
                firstOption.classList.add('highlighted');
            }
        }
    }

    function renderDropdownOptions(dropdown, currencies, type) {
        dropdown.innerHTML = '';

        if (currencies.length === 0) {
            const noResults = document.createElement('div');
            noResults.className = 'currency-dropdown-option no-results';
            noResults.textContent = 'No currencies found';
            dropdown.appendChild(noResults);
            return;
        }

        currencies.forEach(currency => {
            const option = document.createElement('div');
            option.className = 'currency-dropdown-option';
            option.textContent = currency;
            option.addEventListener('click', () => {
                const searchBox = type === 'from' ? fromCurrencySearch : toCurrencySearch;
                searchBox.value = currency;
                if (type === 'from') {
                    fromCurrency = currency;
                    baseCurrency = currency;
                    localStorage.setItem('baseCurrency', currency);
                } else {
                    toCurrency = currency;
                }
                hideCurrencyDropdown(type);
                updateResult();
                fetchHistoricalData();
                if (type === 'from') {
                    renderFavorites();
                    renderAvailableCurrencies();
                }
            });
            dropdown.appendChild(option);
        });
    }

    function showCurrencyDropdown(type, showAll = false) {
        const dropdown = type === 'from' ? fromCurrencyDropdown : toCurrencyDropdown;
        const searchBox = type === 'from' ? fromCurrencySearch : toCurrencySearch;
        const wrapper = searchBox.closest('.currency-select-wrapper');

        wrapper.classList.add('active');
        dropdown.classList.add('show');

        if (showAll || searchBox.value === '') {
            renderDropdownOptions(dropdown, allCurrencies, type);
        } else {
            filterCurrencyOptions(type);
        }
    }

    function hideCurrencyDropdown(type) {
        const dropdown = type === 'from' ? fromCurrencyDropdown : toCurrencyDropdown;
        const searchBox = type === 'from' ? fromCurrencySearch : toCurrencySearch;
        const wrapper = searchBox.closest('.currency-select-wrapper');

        wrapper.classList.remove('active');
        dropdown.classList.remove('show');
    }

    function handleDropdownKeydown(e, type) {
        const dropdown = type === 'from' ? fromCurrencyDropdown : toCurrencyDropdown;
        const options = dropdown.querySelectorAll('.currency-dropdown-option:not(.no-results)');
        const highlighted = dropdown.querySelector('.currency-dropdown-option.highlighted');
        let index = Array.from(options).indexOf(highlighted);

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (index < options.length - 1) {
                if (highlighted) highlighted.classList.remove('highlighted');
                options[index + 1].classList.add('highlighted');
                options[index + 1].scrollIntoView({ block: 'nearest' });
            }
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (index > 0) {
                if (highlighted) highlighted.classList.remove('highlighted');
                options[index - 1].classList.add('highlighted');
                options[index - 1].scrollIntoView({ block: 'nearest' });
            }
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (highlighted) {
                highlighted.click();
            }
        }
    }

    function toggleEditFavorites() {
        if (editFavoritesSection.style.display === 'none') {
            editFavoritesSection.style.display = 'block';
            toggleEditFavoritesBtn.textContent = 'Cancel';
            renderAvailableCurrencies();
        } else {
            editFavoritesSection.style.display = 'none';
            toggleEditFavoritesBtn.textContent = 'Manage Favorites';
        }
    }

    async function fetchExchangeRates() {
        try {
            const response = await fetch('https://api.frankfurter.app/latest');
            if (!response.ok) throw new Error('Failed to fetch rates');

            const data = await response.json();
            exchangeRates = data.rates;
            exchangeRates[data.base] = 1;
            allCurrencies = Object.keys(exchangeRates).sort();

            fromCurrencySearch.value = baseCurrency;
            toCurrencySearch.value = favorites.includes('EUR') ? 'EUR' : favorites[0] || 'USD';
            toCurrency = toCurrencySearch.value;

            renderFavorites();
            fetchHistoricalData();
        } catch (error) {
            console.error('Error fetching exchange rates:', error);
            showError('Error loading exchange rates. Please try again later.');
        }
    }

    async function fetchHistoricalData() {
        try {
            const endDate = new Date();
            const startDate = new Date();
            startDate.setDate(endDate.getDate() - 30);

            const response = await fetch(
                `https://api.frankfurter.app/${formatDate(startDate)}..${formatDate(endDate)}?from=${baseCurrency}&to=${toCurrency}`
            );
            if (!response.ok) throw new Error('Failed to fetch historical data');

            const data = await response.json();
            chartTitle.textContent = `${baseCurrency} to ${toCurrency}`;
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

        const values = Object.values(data.rates).map(rate => rate[toCurrency]);

        if (chart) {
            chart.destroy();
        }

        const ctx = chartCanvas.getContext('2d');
        chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: `${baseCurrency}/${toCurrency}`,
                    data: values,
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.05)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#ffffff',
                    pointBorderColor: '#3b82f6',
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        backgroundColor: '#1f2937',
                        titleFont: {
                            size: 14,
                            weight: 'bold'
                        },
                        bodyFont: {
                            size: 13
                        },
                        padding: 12,
                        displayColors: false,
                        callbacks: {
                            label: function (context) {
                                return `1 ${baseCurrency} = ${context.parsed.y.toFixed(4)} ${toCurrency}`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#6b7280'
                        }
                    },
                    y: {
                        grid: {
                            color: '#e5e7eb'
                        },
                        ticks: {
                            color: '#6b7280'
                        }
                    }
                },
                interaction: {
                    mode: 'nearest',
                    axis: 'x',
                    intersect: false
                }
            }
        });
    }

    function renderFavorites() {
        if (!favoritesContainer) return;

        favoritesContainer.innerHTML = '';

        favorites.forEach(currency => {
            if (currency !== baseCurrency) {
                const rate = getExchangeRate(baseCurrency, currency);
                const favoriteCard = document.createElement('div');
                favoriteCard.className = 'favorite-card';
                favoriteCard.innerHTML = `
                    <div>
                        <div class="favorite-pair">${baseCurrency}/${currency}</div>
                        <div class="favorite-rate">${rate.toFixed(4)}</div>
                    </div>
                    <button class="delete-favorite" data-currency="${currency}">✕</button>
                `;

                favoriteCard.addEventListener('click', (e) => {
                    if (!e.target.classList.contains('delete-favorite')) {
                        toCurrencySearch.value = currency;
                        toCurrency = currency;
                        updateResult();
                        fetchHistoricalData();
                    }
                });

                const deleteBtn = favoriteCard.querySelector('.delete-favorite');
                deleteBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    removeFavorite(currency);
                });

                favoritesContainer.appendChild(favoriteCard);
            }
        });
    }

    function removeFavorite(currency) {
        favorites = favorites.filter(fav => fav !== currency);
        localStorage.setItem('currencyFavorites', JSON.stringify(favorites));
        renderFavorites();
    }

    function getExchangeRate(fromCurrency, toCurrency) {
        return exchangeRates[toCurrency] / exchangeRates[fromCurrency];
    }

    function convertCurrency(event) {
        event.preventDefault();
        updateResult();
    }

    function swapCurrencies() {
        const temp = fromCurrency;
        fromCurrency = toCurrency;
        toCurrency = temp;

        fromCurrencySearch.value = fromCurrency;
        toCurrencySearch.value = toCurrency;

        baseCurrency = fromCurrency;
        localStorage.setItem('baseCurrency', baseCurrency);

        updateResult();
        fetchHistoricalData();
        renderFavorites();
        renderAvailableCurrencies();
    }

    function updateResult() {
        const amount = parseFloat(amountInput.value) || 1;
        const rate = getExchangeRate(fromCurrency, toCurrency);
        const result = amount * rate;

        rateSpan.textContent = `1 ${fromCurrency} = ${rate.toFixed(6)} ${toCurrency}`;
        resultSpan.textContent = `${amount.toFixed(2)} ${fromCurrency} = ${result.toFixed(2)} ${toCurrency}`;
        resultDiv.classList.add('visible');
    }

    function filterCurrencies() {
        const searchTerm = currencySearch.value.toLowerCase();
        const currencyItems = document.querySelectorAll('.currency-item:not(.base)');

        currencyItems.forEach(item => {
            const currency = item.textContent.toLowerCase();
            item.style.display = currency.includes(searchTerm) ? 'flex' : 'none';
        });
    }

    function renderAvailableCurrencies() {
        const container = document.getElementById('availableCurrencies');
        if (!container) return;

        container.innerHTML = '';

        const baseItem = document.createElement('div');
        baseItem.className = 'currency-item base';
        baseItem.textContent = `${baseCurrency} (Base)`;
        baseItem.addEventListener('click', () => { });
        container.appendChild(baseItem);

        allCurrencies.forEach(currency => {
            if (currency !== baseCurrency) {
                const isFavorite = favorites.includes(currency);
                const currencyItem = document.createElement('div');
                currencyItem.className = `currency-item ${isFavorite ? 'selected' : ''}`;

                currencyItem.innerHTML = `
                    <span>${currency}</span>
                    <button class="${isFavorite ? 'remove-currency' : 'add-currency'}">${isFavorite ? 'Remove' : 'Add'}</button>
                `;

                currencyItem.addEventListener('click', (e) => {
                    if (!e.target.classList.contains('add-currency') && !e.target.classList.contains('remove-currency')) {
                        fromCurrencySearch.value = currency;
                        fromCurrency = currency;
                        baseCurrency = currency;
                        localStorage.setItem('baseCurrency', currency);
                        updateResult();
                        fetchHistoricalData();
                        renderFavorites();
                        renderAvailableCurrencies();
                    }
                });

                const actionBtn = currencyItem.querySelector('button');
                actionBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    toggleFavorite(currency);
                });

                container.appendChild(currencyItem);
            }
        });
    }

    function toggleFavorite(currency) {
        const index = favorites.indexOf(currency);
        if (index === -1) {
            favorites.push(currency);
        } else {
            favorites.splice(index, 1);
        }
        renderAvailableCurrencies();
    }

    function saveFavorites() {
        if (favorites.length < 1) {
            showError('Please select at least 1 currency');
            return;
        }

        localStorage.setItem('currencyFavorites', JSON.stringify(favorites));
        renderFavorites();
        editFavoritesSection.style.display = 'none';
        toggleEditFavoritesBtn.textContent = 'Manage Favorites';
    }

    function showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message';
        errorDiv.textContent = message;
        errorDiv.style.color = '#ef4444';
        errorDiv.style.marginTop = '10px';
        errorDiv.style.fontWeight = '600';

        const existingError = document.querySelector('.error-message');
        if (existingError) existingError.remove();

        document.querySelector('.edit-favorites-section').appendChild(errorDiv);
    }
});