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
    const cancelEditBtn = document.getElementById('cancelEdit');
    const saveFavoritesBtn = document.getElementById('saveFavorites');
    const favoritesContainer = document.getElementById('favoritesContainer');
    const chartCanvas = document.getElementById('exchangeChart');
    const currencySearch = document.getElementById('currencySearch');

    let favorites = ['EUR', 'USD', 'GBP', 'RON'];
    let exchangeRates = {};
    let chart = null;
    let allCurrencies = [];
    let baseCurrency = 'RON';

    init();

    function init() {
        loadFavorites();
        setupEventListeners();
        fetchExchangeRates();
    }

    function loadFavorites() {
        const savedFavorites = localStorage.getItem('currencyFavorites');
        if (savedFavorites) {
            favorites = JSON.parse(savedFavorites);
        }
        const savedBase = localStorage.getItem('baseCurrency');
        if (savedBase) {
            baseCurrency = savedBase;
        }
    }

    function setupEventListeners() {
        convertButton.addEventListener('click', convertCurrency);
        swapButton.addEventListener('click', swapCurrencies);
        editFavoritesBtn.addEventListener('click', openEditModal);
        closeModalBtn.addEventListener('click', closeEditModal);
        cancelEditBtn.addEventListener('click', closeEditModal);
        saveFavoritesBtn.addEventListener('click', saveFavorites);
        fromCurrencySelect.addEventListener('change', updateFromCurrency);
        toCurrencySelect.addEventListener('change', updateToCurrency);
        amountInput.addEventListener('input', updateResult);
        currencySearch.addEventListener('input', filterCurrencies);
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
            showError('Error loading exchange rates. Please try again later.');
        }
    }

    async function fetchHistoricalData() {
        try {
            const endDate = new Date();
            const startDate = new Date();
            startDate.setDate(endDate.getDate() - 30);

            const response = await fetch(
                `https://api.frankfurter.app/${formatDate(startDate)}..${formatDate(endDate)}?from=${baseCurrency}&to=${toCurrencySelect.value}`
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

        const values = Object.values(data.rates).map(rate => rate[toCurrencySelect.value]);

        if (chart) {
            chart.destroy();
        }

        chart = new Chart(chartCanvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: `${baseCurrency}/${toCurrencySelect.value}`,
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
                                return `1 ${baseCurrency} = ${context.parsed.y.toFixed(4)} ${toCurrencySelect.value}`;
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
        toCurrencySelect.value = favorites.includes('EUR') ? 'EUR' : favorites[0] || 'USD';
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
                    <div class="favorite-pair">${baseCurrency}/${currency}</div>
                    <div class="favorite-rate">${rate.toFixed(4)}</div>
                `;

                favoriteCard.addEventListener('click', () => {
                    toCurrencySelect.value = currency;
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
        const newBase = toCurrencySelect.value;
        toCurrencySelect.value = baseCurrency;
        baseCurrency = newBase;
        fromCurrencySelect.value = baseCurrency;
        localStorage.setItem('baseCurrency', baseCurrency);

        updateResult();
        fetchHistoricalData();
        renderFavorites();
    }

    function updateFromCurrency() {
        baseCurrency = fromCurrencySelect.value;
        localStorage.setItem('baseCurrency', baseCurrency);
        updateResult();
        fetchHistoricalData();
        renderFavorites();
    }

    function updateToCurrency() {
        updateResult();
        fetchHistoricalData();
    }

    function updateResult() {
        const amount = parseFloat(amountInput.value) || 1;
        const rate = getExchangeRate(baseCurrency, toCurrencySelect.value);
        const result = amount * rate;

        rateSpan.textContent = `1 ${baseCurrency} = ${rate.toFixed(6)} ${toCurrencySelect.value}`;
        resultSpan.textContent = `${amount.toFixed(2)} ${baseCurrency} = ${result.toFixed(2)} ${toCurrencySelect.value}`;
        resultDiv.classList.add('visible');
    }

    function openEditModal() {
        modal.classList.add('active');
        renderAvailableCurrencies();
        document.body.style.overflow = 'hidden';
    }

    function closeEditModal() {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }

    function filterCurrencies() {
        const searchTerm = currencySearch.value.toLowerCase();
        const currencyOptions = document.querySelectorAll('.currency-option');

        currencyOptions.forEach(option => {
            const currency = option.textContent.toLowerCase();
            option.style.display = currency.includes(searchTerm) ? 'flex' : 'none';
        });
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
    }

    function saveFavorites() {
        const selectedCurrencies = Array.from(
            document.querySelectorAll('.currency-option input:checked')
        ).map(checkbox => checkbox.value);

        if (selectedCurrencies.length < 1) {
            showError('Please select at least 1 currency');
            return;
        }

        favorites = selectedCurrencies;
        localStorage.setItem('currencyFavorites', JSON.stringify(favorites));
        renderFavorites();
        closeEditModal();
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

        document.querySelector('.modal-content').appendChild(errorDiv);
    }

    window.addEventListener('click', function (event) {
        if (event.target === modal) {
            closeEditModal();
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && modal.classList.contains('active')) {
            closeEditModal();
        }
    });
});