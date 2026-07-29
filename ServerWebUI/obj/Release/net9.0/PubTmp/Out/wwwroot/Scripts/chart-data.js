'use strict';

$(document).ready(function () {
   
    function generateData(baseval, count, yrange) {
        var i = 0;
        var series = [];
        while (i < count) {
            var x = Math.floor(Math.random() * (750 - 1 + 1)) + 1;;
            var y = Math.floor(Math.random() * (yrange.max - yrange.min + 1)) + yrange.min;
            var z = Math.floor(Math.random() * (75 - 15 + 1)) + 15;

            series.push([x, y, z]);
            baseval += 86400000;
            i++;
        }
        return series;
    }

    window.renderStackedColumnChart = (elementId, series, categories) => {
        var options = {
            chart: {
                type: 'bar',
                height: 400,
                stacked: true,
                toolbar: {
                    show: true
                },
                zoom: {
                    enabled: true
                }
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    columnWidth: '85%', // Increased from 55% to 90%
                    endingShape: 'rounded',
                     dataLabels: {
                        position: 'center'     // Keeps label inside the bar
                    },
                },
            },

            dropShadow: {
                enabled: false             // Prevents label cutoff
            },
            dataLabels: {
                enabled: true,
                style: {
                    fontSize: '9px',           // Adjust size to fit
                    fontWeight: 'bold',
                    colors: ['#ffffff']
                },
            },
            formatter: function (val) {
                return val.toFixed(2);     // Format to 2 decimals if needed
            },
            stroke: {
                show: true,
                width: 1, // Thinner stroke (optional)
                colors: ['transparent']
            },

            series: series,
            xaxis: {
                categories: categories
            },
            yaxis: {
                max: 100,
                title: {
                    text: 'Referral %'
                },
                labels: {
                    formatter: function (val) {
                        return val + "%";
                    },
                    style: {
                        fontSize: '10px' // Reduced font size for y-axis
                    }
                }
            },
            fill: {
                opacity: 1
            },
            tooltip: {
                y: {
                    formatter: function (val) {
                        return val + "%";
                    }
                }
            }
        };

        var chart = new ApexCharts(document.querySelector("#" + elementId), options);
        chart.render();
    };
    
 
    
    window.renderEnrollmentStyledColumnChart = (elementId, values, categories) => {
        const options = {
            chart: {
                type: 'bar',
                height: 350,
                fontFamily: 'Poppins, sans-serif',
                zoom: {
                    enabled: true,
                    type: 'x', // zoom only along x-axis; use 'xy' for both
                    autoScaleYaxis: true
                },
                toolbar: {
                    show: true,
                    tools: {
                        download: true,                      
                        customIcons: []
                    }
                }
            },
            series: [{
                name: "Enrollment Stats",
                data: values
            }],
            xaxis: {
                categories: categories
            },
            plotOptions: {
                bar: {
                    columnWidth: '60%',
                    distributed: true,
                    endingShape: 'rounded'
                }
            },
            dataLabels: {
                enabled: true
            },
            fill: {
                opacity: 1,
                colors: ['#008FFB', '#00E396', '#FEB019', '#fe8a7d', '#FF4560']
            },
            colors: ['#008FFB', '#00E396', '#FEB019', '#fe8a7d', '#FF4560'],
            tooltip: {
                y: {
                    formatter: val => val + " patients"
                }
            },
            legend: {
                show: false
            }
        };

        const chart = new ApexCharts(document.querySelector(`#${elementId}`), options);
        if (window.enrollmentChartInstance) {
            window.enrollmentChartInstance.destroy();
        }

        chart.render();
        window.enrollmentChartInstance = chart;
    };



    window.renderDonutChart = (elementId, values, labels) => {
        const options = {
            chart: {
                type: 'donut',
                height: 400,
                fontFamily: 'Poppins, sans-serif',
                toolbar: {
                    show: true
                },
                animations: {
                    enabled: true,
                    easing: 'easeinout',
                    speed: 800,
                    animateGradually: {
                        enabled: true,
                        delay: 150
                    },
                    dynamicAnimation: {
                        enabled: true,
                        speed: 350
                    }
                }
            },
            series: values,
            labels: labels,
            dataLabels: {
                enabled: true,
                formatter: function (val, opts) {
                    return val.toFixed(1) + '%';
                },
                dropShadow: {
                    enabled: true,
                    top: 1,
                    left: 1,
                    blur: 1,
                    opacity: 0.45
                },
                style: {
                    fontSize: '11px',
                    fontWeight: 'bold'
                }
            },
            tooltip: {
                y: {
                    formatter: function (value) {
                        return value + " patients";
                    }
                }
            },
            legend: {
                position: 'bottom',
                fontSize: '14px',
                labels: {
                    colors: '#000'
                },
                markers: {
                    width: 14,
                    height: 14,
                    radius: 5
                },
                itemMargin: {
                    horizontal: 10,
                    vertical: 4
                }
            },
            plotOptions: {
                pie: {
                    donut: {
                        size: '65%',
                        labels: {
                            show: false,
                            
                        }
                    }
                }
            },
            fill: {
                type: 'gradient',
            },
            stroke: {
                show: true,
                width: 2,
                colors: ['#fff']
            },
            colors: ['#00E396', '#008FFB', '#FEB019', '#FF4560', '#fe8a7d', '#546E7A'],
            responsive: [{
                breakpoint: 480,
                options: {
                    chart: {
                        height: 300
                    },
                    legend: {
                        fontSize: '11px'
                    }
                }
            }]
        };

        const chart = new ApexCharts(document.querySelector(`#${elementId}`), options);
        if (window.donutChartInstance) {
            window.donutChartInstance.destroy();
        }
        chart.render();

        window.donutChartInstance = chart;
    };
    

});


    $(document).ready(function () {

        function generateData(baseval, count, yrange) {
            var i = 0;
            var series = [];
            while (i < count) {
                var x = Math.floor(Math.random() * (750 - 1 + 1)) + 1;;
                var y = Math.floor(Math.random() * (yrange.max - yrange.min + 1)) + yrange.min;
                var z = Math.floor(Math.random() * (75 - 15 + 1)) + 15;

                series.push([x, y, z]);
                baseval += 86400000;
                i++;
            }
            return series;
        }

        

        window.renderLineChart = (elementId, seriesData, categories) => {
            const options = {
                chart: {
                    type: 'line',
                    height: 450,
                    fontFamily: 'Poppins, sans-serif',
                    toolbar: {
                        show: true,
                        tools: {
                            download: true,
                            selection: false,
                            zoom: false,
                            zoomin: false,
                            zoomout: false,
                            pan: false,
                            reset: false
                        }
                    }
                },
                stroke: {
                    curve: 'smooth',
                    width: 3
                },
                colors: ['#4caf50', '#2196f3', '#ff9800', '#9c27b0'], // Green, Blue, Orange, Purple
                series: seriesData,

                xaxis: {
                    categories: categories,

                    labels: {
                        rotate: -45,
                        style: {
                            fontSize: '12px'
                        }
                    },
                    axisBorder: {
                        show: true,
                        color: '#999'
                    }
                },
                yaxis: {
                    title: {
                        text: 'Total Registrations',
                        style: {
                            fontSize: '14px',
                            fontWeight: 600
                        }
                    },
                    labels: {
                        formatter: function (val) {
                            return Math.round(val);
                        },
                        style: {
                            fontSize: '12px'
                        }
                    }
                },
                tooltip: {
                    shared: true,
                    intersect: false,
                    theme: 'light',
                    style: {
                        fontSize: '13px'
                    }
                },
                dataLabels: {
                    enabled: false
                },
                markers: {
                    size: 5,
                    hover: {
                        sizeOffset: 4
                    }
                },
                legend: {
                    position: 'bottom',
                    horizontalAlign: 'center',
                    fontSize: '12px',
                    fontWeight: 500,
                    markers: {
                        radius: 12
                    }
                },
                grid: {
                    row: {
                        colors: ['#f9f9f9', '#fff'], // Alternating row background
                        opacity: 1
                    },
                    borderColor: '#e0e0e0'
                }
            };

            const chartElement = document.querySelector(`#${elementId}`);
            chartElement.innerHTML = ''; // Clear previous chart
            const chart = new ApexCharts(chartElement, options);
            chart.render();
        };



        window.renderStackedBarChart = (elementId, categories, maleValues, femaleValues) => {
            const options = {
                chart: {
                    type: 'bar',
                    height: 400,
                    stacked: true,
                    toolbar: { show: true },
                    fontFamily: 'Poppins, sans-serif'
                },
                plotOptions: {
                    bar: {
                        horizontal: false,
                        columnWidth: '65%',
                        endingShape: 'rounded'
                    }
                },
                series: [
                    { name: 'Male', data: maleValues },
                    { name: 'Female', data: femaleValues }
                ],
                xaxis: {
                    categories: categories,
                    labels: { style: { fontSize: '14px', fontWeight: 600 } }
                },
                yaxis: {
                    labels: { style: { fontSize: '13px' } }
                },
                legend: {
                    position: 'bottom',
                    fontWeight: 600
                },
                fill: { opacity: 1 },
                colors: ['#008FFB', '#FF4560'],
                tooltip: {
                    y: { formatter: (val) => val + " patients" }
                },
                dataLabels: {
                    enabled: true,
                    style: { fontSize: '12px', fontWeight: 'bold', colors: ["#fff"] }
                }
            };

            if (window.stackedBarChartInstance) {
                window.stackedBarChartInstance.destroy();
            }

            const chart = new ApexCharts(document.querySelector(`#${elementId}`), options);
            chart.render();
            window.stackedBarChartInstance = chart;
        };



        window.renderStackedBarHorizontalChart = (elementId, categories, maleValues, femaleValues) => {
            const options = {
                chart: {
                    type: 'bar',
                    height: 450,
                    stacked: true,
                    fontFamily: 'Poppins, sans-serif',
                    toolbar: { show: true, },
                },
                plotOptions: {
                    bar: {
                        horizontal: true,           // ✅ horizontal bars
                        barHeight: '50%',
                        endingShape: 'rounded'
                    }
                },
                series: [
                    {
                        name: 'Male %',
                        data: maleValues
                    },
                    {
                        name: 'Female %',
                        data: femaleValues
                    }
                ],
                colors: ['#196619', '#FF4560'],   // Blue = Male, Red = Female
                xaxis: {
                    categories: categories,
                    labels: {
                        style: { fontSize: '14px', fontWeight: 600 }
                    },
                    title: { text: 'Patients Per Cluster %', style: { fontSize: '14px', fontWeight: 600 } }
                },
                yaxis: {
                    labels: {
                        style: { fontSize: '14px', fontWeight: 600 }
                    }
                },
                tooltip: {
                    y: {
                        formatter: (val) => val + "%"
                    }
                },
                legend: {
                    position: 'top',
                    fontSize: '14px',
                    fontWeight: 600
                },
                fill: {
                    type: 'gradient',
                    gradient: {
                        shade: 'Dark',
                        type: "horizontal",
                        shadeIntensity: 0.25,
                        gradientToColors: undefined,
                        inverseColors: true,
                        opacityFrom: 0.85,
                        opacityTo: 0.85,
                        stops: [50, 0, 100]
                    }
                },
                dataLabels: {
                    enabled: true,
                    formatter: (val) => val,
                    style: { colors: ['#fff'], fontSize: '12px', fontWeight: 'bold' }
                },
                responsive: [
                    {
                        breakpoint: 480,
                        options: {
                            chart: { height: 300 },
                            yaxis: { labels: { style: { fontSize: '12px' } } }
                        }
                    }
                ]
            };

            // Destroy previous instance
            if (window.horizontalStackedBarInstance) {
                window.horizontalStackedBarInstance.destroy();
            }

            const chart = new ApexCharts(document.querySelector(`#${elementId}`), options);
            chart.render();
            window.horizontalStackedBarInstance = chart;
        };


    });



















    



   
   



