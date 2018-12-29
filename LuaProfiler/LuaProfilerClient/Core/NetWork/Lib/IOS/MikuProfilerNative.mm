float getiOSBatteryLevel()
{
    [[UIDevice currentDevice] setBatteryMonitoringEnabled:YES];
    return [[UIDevice currentDevice] batteryLevel];
}