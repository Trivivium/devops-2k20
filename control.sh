if [ "$1" = "init" ]; then
    echo "Skipped... Initialization is handles by the application during startup."
elif [ "$1" = "start" ]; then
    echo "Starting minitwit..."
    nohup dotnet run ./src/WebApplication/WebApplication.csproj > /tmp/out.log 2>&1 &
    echo $! > /tmp/minitwit.pid
elif [ "$1" = "stop" ]; then
    echo "Stopping minitwit..."
    MINITWIT_PID=$(cat /tmp/minitwit.pid)
    kill -TERM "$MINITWIT_PID"
    rm /tmp/minitwit.pid
elif [ "$1" = "inspectdb" ]; then
    ./flag_tool -i | less
elif [ "$1" = "flag" ]; then
    ./flag_tool "$@"
else
  echo "I do not know this command..."
fi


