document.addEventListener('DOMContentLoaded', function () {
    const toggleSidebar = document.getElementById('toggle-sidebar');
    const sidebar = document.getElementById('sidebar');
    const content = document.querySelector('.main-content-wrapper');
    const footer = document.querySelector('.page-footer');

    toggleSidebar.addEventListener('click', function () {
        sidebar.classList.toggle('sidebar-hidden');
        content.classList.toggle('sidebar-hidden');
        footer.classList.toggle('sidebar-hidden');
    });
});