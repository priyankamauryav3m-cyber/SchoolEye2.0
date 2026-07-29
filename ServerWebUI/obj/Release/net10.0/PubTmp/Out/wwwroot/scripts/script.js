
window.initBodyLoad = () => {

    // Click Menu Button
    function toggleMenu() {
        //debugger;
        let widthCheck = $(window).width();

        if (widthCheck > 568) {
            $("body").toggleClass("full-screen");
        }

        else {
            $("body").toggleClass("mobile-full-screen");
        }
    }
    $(document).on("click", ".quickgrid tbody tr", function () {
        $(".quickgrid tbody tr").removeClass("selectRow");
        $(this).addClass("selectRow");
    });
    

    $("#menu-btn").click(function (event) {
        event.stopPropagation();
        toggleMenu();
    });

    $(document).click(function (event) {
        if (!$(event.target).closest("#menu-btn, .side-menu").length) {
            $("body").removeClass("mobile-full-screen");
        }
    });

    $(window).resize(function () {
        //debugger
        let widthCheck = $(window).width();
        if (widthCheck > 568) {
            $("body").removeClass("mobile-full-screen");
        }
        else {
            $("body").removeClass("full-screen");
        }
    });

    function checkScreenSize() {
        let width = $(window).width();

        if (width < 1000 && width > 568) {
            $("body").addClass("full-screen auto-full-screen").removeClass("mobile-screen");
        } else if (width <= 568) {
            $("body").addClass("mobile-screen").removeClass("full-screen auto-full-screen");
        } else {
            $("body").removeClass("full-screen mobile-screen auto-full-screen");
        }
    }
    $(document).ready(checkScreenSize);
    $(window).resize(checkScreenSize);

    //FullScreen
    var elem = document.documentElement;
    var Fullscreen = document.getElementById("openFullscreen");
    var exitscreen = document.getElementById("exitFullscreen");

    $(Fullscreen).click(function () {
        if (elem.requestFullscreen) {
            elem.requestFullscreen();
        } else if (elem.webkitRequestFullscreen) {
            elem.webkitRequestFullscreen();
        } else if (elem.msRequestFullscreen) {
            elem.msRequestFullscreen();
        }
        Fullscreen.style.display = "none";
        exitscreen.style.display = "flex";
    });

    $(exitscreen).click(function () {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
        Fullscreen.style.display = "flex";
        exitscreen.style.display = "none";
    });

    // Sidebar Hover 
    $(".sidebar-menu").mouseenter(function () {
        $(".side-menu").addClass("move-menu");
    }).mouseleave(function () {
        $(".side-menu").removeClass("move-menu");
    });

    // Hover to open nav menu name Tooltip
    $('[data-toggle="tooltip"]').tooltip();

    // Open overlay box
    $('.overlay-open').on('show.bs.dropdown', function () {
        $('body').addClass('open-dropdown');
    });

    $('.overlay-open').on('hide.bs.dropdown', function () {
        $('body').removeClass('open-dropdown');
    });

};

//Login Carousel 
window.initCarousel = () => {
    var myCarouselElement = document.querySelector('#loginCarousel');
    if (myCarouselElement) {
        new bootstrap.Carousel(myCarouselElement);
    }
};

// Important Link Hide Show 
window.importantLink = () => {
    $("#important-hide-btn").click(function () {
        $('body').toggleClass("important-hide");
    });
};

window.menuMap = function () {

    $(document).off('click', '.open-menu');

    $(document).on('click', '.open-menu', function (e) {

        e.stopPropagation();

        let $li = $(this).closest('li');

        let $nextTree = $li.children('ul.tree');

        let $icon = $li.children('.branch').find('i.bi');

        if (!$nextTree.length) return;

        if ($nextTree.is(':visible')) {

            $nextTree.find('ul.tree')

                .slideUp(200)

                .removeClass('d-flex');

            $nextTree.slideUp(400, function () {

                $(this).removeClass('d-flex');

                $li.removeClass('active-tree');

            });

            $icon.removeClass('bi-dash-lg').addClass('bi-plus-lg');

        } else {

            $li.addClass('active-tree');

            $nextTree

                .addClass('d-flex')

                .hide()

                .slideDown(400);

            $icon.removeClass('bi-plus-lg').addClass('bi-dash-lg');

        }

    });

};




//Calendar
window.initCalendar = () => {
    const currentDate = document.querySelector(".current-date"),
        prevBtn = document.getElementById("prev"),
        nextBtn = document.getElementById("next"),
        daysTag = document.querySelector(".days"),
        weekDays = document.querySelectorAll(".week-name");

    let date = new Date(),
        currYear = date.getFullYear(),
        currMonth = date.getMonth();

    const months = ["January", "February", "March", "April", "May", "June", "July",
        "August", "September", "October", "November", "December"];

    function renderCalendar() {
        let firstDayofMonth = new Date(currYear, currMonth, 1).getDay(),
            lastDateofMonth = new Date(currYear, currMonth + 1, 0).getDate(),
            lastDateofLastMonth = new Date(currYear, currMonth, 0).getDate();

        let tableHTML = "";
        let dayCount = 1;
        let nextMonthDayCount = 1;

        for (let row = 0; row < 6; row++) {
            let rowHTML = "<tr>";

            for (let col = 0; col < 7; col++) {
                if (row === 0 && col < firstDayofMonth) {
                    rowHTML += `<td class="inactive">${lastDateofLastMonth - firstDayofMonth + col + 1}</td>`;
                } else if (dayCount > lastDateofMonth) {
                    rowHTML += `<td class="inactive">${nextMonthDayCount++}</td>`;
                } else {
                    let isToday = dayCount === new Date().getDate() && currMonth === new Date().getMonth()
                        && currYear === new Date().getFullYear() ? "active" : "";
                    rowHTML += `<td class="${isToday}">${dayCount++}</td>`;
                }
            }

            rowHTML += "</tr>";
            tableHTML += rowHTML;
        }

        currentDate.innerText = `${months[currMonth]} ${currYear}`;
        daysTag.innerHTML = tableHTML;

        highlightCurrentWeek();
    }

    function highlightCurrentWeek() {
        let todayIndex = new Date().getDay();
        weekDays.forEach((week, index) => {
            if (index === todayIndex) {
                week.classList.add("active-week");
            } else {
                week.classList.remove("active-week");
            }
        });
    }

    prevBtn.addEventListener("click", () => {
        currMonth--;
        if (currMonth < 0) {
            currMonth = 11;
            currYear--;
        }
        renderCalendar();
    });

    nextBtn.addEventListener("click", () => {
        currMonth++;
        if (currMonth > 11) {
            currMonth = 0;
            currYear++;
        }
        renderCalendar();
    });

    renderCalendar();
};
