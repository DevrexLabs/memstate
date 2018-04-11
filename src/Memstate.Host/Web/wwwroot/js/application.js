$(function () {
    var fetchMetrics = function () {
        $.ajax({
            url: '/metrics',
            dataType: 'json',
            success: function (data) {
                for (var i = 0; i < data.contexts.length; i++) {
                    var context = data.contexts[i];

                    for (var j = 0; j < context.counters.length; j++) {
                        var counter = context.counters[j];

                        if (counter.name === "CommandsExecuted") {
                            $('div#metrics p.commands-executed span.value').text(counter.count);
                        }

                        if (counter.name === "QueriesExecuted") {
                            $('div#metrics p.queries-executed span.value').text(counter.count);
                        }

                        if (counter.name === "PendingCommands") {
                            $('div#metrics p.pending-commands span.value').text(counter.count);
                        }
                    }

                    for (j = 0; j < context.gauges.length; j++) {
                        var gauge = context.gauges[j];

                        if (gauge.name === "PendingCommands") {
                            $('div#metrics p.pending-commands span.value').text(gauge.value);
                        }
                    }

                    for (j = 0; j < context.timers.length; j++) {
                        var timer = context.timers[j];

                        if (timer.name === "CommandExecutionTime") {
                            $('div#metrics p.command-execution-time span.value').text(timer.histogram.mean.toFixed(2) + " " + timer.durationUnit);
                        }

                        if (timer.name === "QueryExecutionTime") {
                            $('div#metrics p.query-execution-time span.value').text(timer.histogram.mean.toFixed(2) + " " + timer.durationUnit);
                        }

                        if (timer.name === "KernelCommandExecutionTime") {
                            $('div#metrics p.kernel-command-execution-time span.value').text(timer.histogram.mean.toFixed(2) + " " + timer.durationUnit);
                        }

                        if (timer.name === "KernelQueryExecutionTime") {
                            $('div#metrics p.kernel-query-execution-time span.value').text(timer.histogram.mean.toFixed(2) + " " + timer.durationUnit);
                        }
                    }
                }
            },
            error: function (xhr, status, message) {
                alert(status + ' ' + message);
            }
        });
    };

    setInterval(fetchMetrics, 1000);
    fetchMetrics();
});