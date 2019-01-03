float GetIOSBatteryLevel()
{
    [[UIDevice currentDevice] setBatteryMonitoringEnabled:YES];
    return [[UIDevice currentDevice] batteryLevel];
}