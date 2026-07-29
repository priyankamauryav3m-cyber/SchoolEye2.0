$(document).ready(function () {

    var elem = document.documentElement;
    var Fullscreen = document.getElementById("openFullscreen");
    var exitscreen = document.getElementById("exitFullscreen");

    $(Fullscreen).click(function () {
        debugger;
        if (elem.requestFullscreen) {
            elem.requestFullscreen();
        } else if (elem.webkitRequestFullscreen) { // Safari
            elem.webkitRequestFullscreen();
        } else if (elem.msRequestFullscreen) { // IE11
            elem.msRequestFullscreen();
        }
        Fullscreen.style.display = "none";
        exitscreen.style.display = "block";
    });

    $(exitscreen).click(function () {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) { // Safari
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) { // IE11
            document.msExitFullscreen();
        }
        Fullscreen.style.display = "block";
        exitscreen.style.display = "none";
    });
});

window.initMainJsFunctions = function () {

    $(".nav-item").hover(
        function () {
            $(this).addClass("active");
        },
        function () {
            $(this).removeClass("active");
        }
    );

    $(".asidebar-nav").hover(
        function () {
            $('.asidebar').addClass("asidebar-new");
        },
        function () {
            $('.asidebar').removeClass("asidebar-new");
        }
    );

    //$(".nav-lin.active").each(function () {
    //    $(this).closest(".nav-item").addClass("child-active");
    //});

    function updateNavState() {
        $(".nav-item").each(function () {
            if ($(this).find(".nav-lin.active").length > 0) {
                $(this).addClass("child-active");
            } else {
                $(this).removeClass("child-active");
            }
        });
    }

    $(document).ready(function () {

        updateNavState();


        $(document).on("click", ".nav-link", function () {
            //debugger;
            $(".nav-lin").removeClass("active");
            //$(this).addClass("active");
            updateNavState();
        });
        $(document).on("click", ".nav-lin", function () {
            //debugger;
            //$(".nav-lin").removeClass("active");
            $(this).addClass("active");
            updateNavState();
        });
    });


    $('[data-toggle="tooltip"]').tooltip();


    //if ($('.toggle-asidebar-btn')) {
    //    $('.toggle-asidebar-btn').on('click', function () {
    //        $('body').toggleClass('toggle-asidebar');
    //    });
    //}
    $(document).ready(function () {
        $('.toggle-asidebar-btn').on('click', function (e) {
            $('body').toggleClass('toggle-asidebar');
            e.stopPropagation();
        });

        $(document).on('click', function (e) {
            if (window.innerWidth < 1200) {
                if (!$(e.target).closest('.sidebar').length && !$(e.target).is('.toggle-asidebar-btn')) {
                    $('body').removeClass('toggle-asidebar');
                }
            }
        });

        $(window).on('resize', function () {
            if (window.innerWidth >= 1200) {
                $('body').removeClass('toggle-asidebar');
            }
        });
    });
};


window.heightAdgust = function () {


    function adjustHeight() {
        $('#h-din').height(
            $(window).height() - $('#top-headings').height() - $('#footer-pagenation').height() - 170
        );
    }
    $(document).ready(adjustHeight);
    $(window).resize(adjustHeight);
    function bodyHeight() {
        var windowHeight = $(window).height();

        $('body').height(windowHeight - 0);
    }

    $(document).ready(bodyHeight);
    $(window).resize(bodyHeight);

}

$(document).ready(function () {
    $(".col-options-button").click(function () {
        debugger;
        $(this).closest(".col-header-content").find(".col-title").hide();
    });
});

function triggerFileInput(inputElement) {
    console.log('File input triggered'); // Log to verify that JS is working
    inputElement.click(); // Trigger the click event on the file input
}
function importData() {
    let input = document.createElement('input');
    input.type = 'file';
    input.click();

}

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++Focus Input Box if Validation not satisfy
window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};


//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++DEVELOPMENT JS
function downloadFileFromStream(fileName, fileStream) {
    const blob = new Blob([fileStream]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName;
    anchorElement.click();
    URL.revokeObjectURL(url);
}
function showdropdown(selector) {
    $(selector).show();
}
function filterDropdown(inputSelector, dropdownSelector) {
    const input = $(inputSelector).val().toLowerCase();
    $(dropdownSelector + ' .dropdown-item').each(function () {
        const text = $(this).text().toLowerCase();
        $(this).toggle(text.includes(input));
    });
}
function showDropdown(selector) {
    // First hide all other open dropdowns
    $('.dropdown-list').hide(); // Hide all dropdowns
    $(selector).show(); // Show the selected dropdown
}
function filterDropdown(inputSelector, dropdownSelector) {
    const input = $(inputSelector).val().toLowerCase();
    $(dropdownSelector + ' .dropdown-item').each(function () {
        const text = $(this).text().toLowerCase();
        $(this).toggle(text.includes(input));
    });
}
function hideDropdown(selector) {
    $(selector).hide();
}

// Close dropdown when clicking outside
$(document).on('click', function (event) {
    if (!$(event.target).closest('.dropdown-input').length) {
        hideDropdown('.dropdown-list');  // Hides all dropdowns when clicking outside
    }
});

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++Dropdown Scroll
function scrollToElement(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ block: "nearest", behavior: "smooth" });
    }
}


//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++Clincal Closing Dropdown



function setupSubmenuToggle(buttonSelector, submenuSelector) {
    $(document).on('click', buttonSelector, function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $submenu = $(submenuSelector);
        $('.submenu').not($submenu).slideUp(200); // Close other submenus
        $submenu.toggle(); // Toggle the current submenu
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest(buttonSelector).length && !$(e.target).closest(submenuSelector).length) {
            $(submenuSelector).slideUp(200); // Close submenu when clicking outside
        }
    });
}


//=================================================================By Aakash

function limitNumberInput(selector) {
    $(selector).each(function () {
        $(this).on('keyup', function () {
            const maxVal = Number($(this).attr("max"));
            const currentVal = $(this).val();

            if (currentVal > maxVal) {
                // Limit the input to the max length of the max value
                const maxLength = String(maxVal).length;
                const newValue = currentVal.slice(0, maxLength);
                $(this).val(newValue);
            }
        });
    });
}
//======================================================Query for change pagination text.

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++Function for Iframe
function myFunction(combinedHtml) {     // Access the HTML string directly   
    var myWindow = window.open("", "", "width=1000,height=1000");
    myWindow.document.open();
    myWindow.document.write(`
        <html>
        <head>
                <link href="../css/newwindow.css" rel="stylesheet" />
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@4.1.3/dist/css/bootstrap.min.css"
        integrity="sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO" crossorigin="anonymous">
            <title>Print Table</title>
            <style>
                body { font-family: Arial, sans-serif; }
                table { width: 100%; border-collapse: collapse; }
                th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                th { background-color: white; }
                @media print {
                    button {
                        display: none !important;
                    }
                    .print-p-0 {
                        padding: 0px !important;
                    }
                    .print-m {
                        margin-top: 5rem !important;
                    }
                }
                .box-key-heading {
                    margin: 0;
                    width: fit-content;
                    padding: 4px;
                    background-color: #d4d4d4;
                    color: #333333;
                    border-radius: 2px;
                    -webkit-print-color-adjust: exact;
                }
                .cust-tr-p {
                    padding: 6px !important;
                }
                .report-nm {
                    text-align: center;
                    font-size: 19px;
                    margin: 0;
                    border-bottom: 2px solid #0c5460;
                    width: fit-content;
                    color: #0c5460;
                    font-family: math;
                    text-decoration: none !important;
                }
            </style>
        </head>
        <body>
        <div class='row m-0 mt-3'>
       
        </div>
            ${combinedHtml}
            <div class="row m-0 pr-0">
            <div class="col-11 p-3 pr-0 d-flex align-items-center justify-content-end print-p-0">
            <button onclick="window.print()" class="btn btn-primary">Print</button>
            </div>
            </div>
        </body>
        </html>
    `);
    myWindow.document.close();
}
//<div class="col-12 py-0 px-2 d-flex align-items-center justify-content-center mb-1 print-100"><h1 class='report-nm'> Clinical Case Sheet</h1></div> 


//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++Function for Scrool screen validation
function scrollToValidationMessage(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "center" });
    }
}

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++For Area
function handleKeyPress1() {
    // Use event delegation by binding the keypress event to the document
    $(document).on('keypress', 'input, select', function (event) {
        if (event.keyCode == 13) {  // ENTER key
            event.preventDefault();  // Prevent form submission
            $("#btn-submit").click();  // Simulate button click
        }
    });
}

//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++Camera Operation used In Registration 


function startVideo(src) {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
            let video = document.getElementById(src);

            if (!video) {
                console.error(`Video element with id '${src}' not found.`);
                return;
            }

            // Set the video stream to srcObject
            if ("srcObject" in video) {
                video.srcObject = stream;
            } else {
                video.src = window.URL.createObjectURL(stream);
            }

            // Play the video once it's loaded
            video.onloadedmetadata = function () {
                video.play();
            };

            // Apply mirror effect (optional)
            video.style.webkitTransform = "scaleX(-1)";
            video.style.transform = "scaleX(-1)";
        }).catch(error => {
            console.error("Error accessing the camera:", error);
        });
    } else {
        console.warn("getUserMedia is not supported by this browser.");
    }
}
function getFrame(src, dest, dotNetHelper) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);

    if (!video || !canvas) {
        console.error("Video or canvas element not found.");
        return;
    }
    canvas.getContext('2d').drawImage(video, 0, 0, 320, 240);
    let dataUrl = canvas.toDataURL("image/jpeg");

    dotNetHelper.invokeMethodAsync('ProcessImage', dataUrl).then(() => {
        console.log("Image processing completed successfully.");
    }).catch(error => {
        console.error("Error during image processing:", error);
        // Ensure video stops even if there's an error
    });
    closeWebcam(src);
}

function closeWebcam(src) {
    // Get the video element by its ID
    const video = document.getElementById(src);

    if (!video) {
        console.error(`Video element with id '${src}' not found.`);
        return;
    }

    // Check if the video element has an active media stream
    if (video.srcObject) {
        const stream = video.srcObject;

        // Stop all tracks (both audio and video)
        const tracks = stream.getTracks();
        tracks.forEach(track => track.stop());

        // Clear the stream from the video element
        video.srcObject = null;
        video.pause();  // Optionally, pause the video
        console.log("Webcam video stream stopped.");
    } else {
        console.warn("No active webcam stream found to stop.");
    }
}

/*-------------------------------------------------------------------*/
function downloadFile(fileName, contentType, base64Data) {
    var link = document.createElement('a');
    link.download = fileName;
    link.href = "data:" + contentType + ";base64," + base64Data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}





