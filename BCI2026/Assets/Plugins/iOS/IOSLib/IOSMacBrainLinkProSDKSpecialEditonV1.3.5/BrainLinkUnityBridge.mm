#import <Foundation/Foundation.h>
#import "Blue4Manager.h"

extern "C" void UnitySendMessage(const char *objectName, const char *methodName, const char *message);

static NSMutableArray<ScannedDevice *> *refreshDevices;

static void SendUnityMessage(NSString *methodName, NSString *message)
{
    UnitySendMessage("ThinkGearManager", methodName.UTF8String, message.UTF8String);
}

static void SendUnityInteger(NSString *methodName, int value)
{
    SendUnityMessage(methodName, [NSString stringWithFormat:@"%d", value]);
}

static void ConfigureCallbacks(void)
{
    Blue4Manager *manager = [Blue4Manager shareInstance];

    manager.blueConBlock = ^(NSString *markKey) {
        if ([markKey isEqualToString:@"1"]) {
            SendUnityMessage(@"ReceiveContentState", @"yes");
        }
    };

    manager.blueDisBlock = ^(NSString *markKey) {
        if ([markKey isEqualToString:@"1"]) {
            SendUnityMessage(@"ReceiveContentState", @"no");
        }
    };

    manager.hzlblueDataBlock_A = ^(HZLBlueData *blueData, BlueType blueType, BOOL isFalseConnection) {
        if (isFalseConnection) {
            return;
        }

        if (blueData.bleDataType == BLEMIND) {
            SendUnityInteger(@"ReceivePoorSignal", blueData.signal);
            SendUnityInteger(@"ReceiveAttention", blueData.attention);
            SendUnityInteger(@"ReceiveMeditation", blueData.meditation);
            SendUnityInteger(@"ReceiveBatteryCapacity", blueType == BlueType_Lite ? 0 : blueData.batteryCapacity);

            if (blueType == BlueType_Pro || blueType == BlueType_Lite) {
                SendUnityInteger(@"ReceiveDelta", blueData.delta);
                SendUnityInteger(@"ReceiveTheta", blueData.theta);
                SendUnityInteger(@"ReceiveLowAlpha", blueData.lowAlpha);
                SendUnityInteger(@"ReceiveHighAlpha", blueData.highAlpha);
                SendUnityInteger(@"ReceiveLowBeta", blueData.lowBeta);
                SendUnityInteger(@"ReceiveHighBeta", blueData.highBeta);
                SendUnityInteger(@"ReceiveLowGamma", blueData.lowGamma);
                SendUnityInteger(@"ReceiveHighGamma", blueData.highGamma);
            }

            if (blueType == BlueType_Pro) {
                SendUnityInteger(@"ReceiveHeaetRate", blueData.heartRate.intValue);
                SendUnityMessage(@"ReceiveTemperature", [NSString stringWithFormat:@"%@", blueData.temperature ?: @"0"]);
                SendUnityInteger(@"ReceiveGrind4_0", blueData.grind.intValue);
                SendUnityInteger(@"ReceiveAp4_0", blueData.ap);
                SendUnityMessage(@"ReceiveHardwareversion4_0", blueData.hardwareVersion ?: @"");

                NSMutableArray<NSString *> *hrvValues = [NSMutableArray array];
                for (NSNumber *value in blueData.HRV ?: @[]) {
                    [hrvValues addObject:[NSString stringWithFormat:@"%@ms", value]];
                }
                if (hrvValues.count > 0) {
                    SendUnityMessage(@"ReceiveHRV", [hrvValues componentsJoinedByString:@","]);
                }
            }
        } else if (blueData.bleDataType == BLEGRAVITY && blueType == BlueType_Pro) {
            SendUnityInteger(@"ReceiveXValue", blueData.xvlaue);
            SendUnityInteger(@"ReceiveYValue", blueData.yvlaue);
            SendUnityInteger(@"ReceiveZValue", blueData.zvlaue);
        } else if (blueData.bleDataType == BLERaw) {
            SendUnityInteger(@"ReceiveRawdata", blueData.raw);
        }
    };
}

static void ReportScannedDevice(ScannedDevice *device)
{
    NSString *identifier = device.peripheral.identifier.UUIDString;
    for (ScannedDevice *knownDevice in refreshDevices) {
        if ([knownDevice.peripheral.identifier.UUIDString isEqualToString:identifier]) {
            return;
        }
    }

    [refreshDevices addObject:device];
    NSString *message = [NSString stringWithFormat:@"%@,%@,%d", device.name ?: @"Unknown", identifier, device.RSSI.intValue];
    SendUnityMessage(@"DeviceFound", message);
}

extern "C" {
    void SetWhiteList(const char *whiteList)
    {
        refreshDevices = [NSMutableArray array];
        NSString *names = [NSString stringWithUTF8String:whiteList ?: ""];
        NSArray<NSString *> *allowedNames = [names componentsSeparatedByString:@","];

        [Blue4Manager logEnable:YES];
        [[Blue4Manager shareInstance] configureBlueNames:allowedNames ableDeviceSum:1];
        ConfigureCallbacks();

        [[Blue4Manager shareInstance] bluePermission:^(BluePermission_state state, NSString *message) {
            if (state == Blue4_PoweredOff || state == Blue4_Unauthorized || state == Blue4_UnknownOrUnsupported) {
                SendUnityMessage(@"ReceiveContentState", @"no");
            }
        }];

        // This initializes the SDK's internal data callback before a manual connection.
        [[Blue4Manager shareInstance] connectBlue4WithIsAuto:NO];
    }

    void Scan(void)
    {
        [refreshDevices removeAllObjects];
        [[Blue4Manager shareInstance] scanBlue4WithScannedWithScanTime:12 blue4DeviceBlock:^(ScannedDevice *device) {
            ReportScannedDevice(device);
        }];
    }

    void ConnectDevice(const char *identifier)
    {
        NSString *requestedIdentifier = [NSString stringWithUTF8String:identifier ?: ""];
        for (ScannedDevice *device in refreshDevices) {
            if ([device.peripheral.identifier.UUIDString isEqualToString:requestedIdentifier]) {
                [[Blue4Manager shareInstance] manuallyConnetBlue4ForScannedDevice:device];
                return;
            }
        }
    }

    void SendSettings(const char *settings)
    {
        NSString *command = [NSString stringWithUTF8String:settings ?: ""];
        NSData *data = [command dataUsingEncoding:NSUTF8StringEncoding];
        [[Blue4Manager shareInstance] writeToProAWithCode:data];
    }
}
