<script>
    const toggleSidebar = document.getElementById('toggle-sidebar');
    const sidebar = document.getElementById('sidebar');
    const content = document.querySelector('.main-content-wrapper');
    const footer = document.querySelector('.page-footer');

    if (toggleSidebar && sidebar && content && footer) {
        toggleSidebar.addEventListener('click', function () {
            sidebar.classList.toggle('sidebar-hidden');
            content.classList.toggle('sidebar-hidden');
            footer.classList.toggle('sidebar-hidden');
        });
        }
</script>