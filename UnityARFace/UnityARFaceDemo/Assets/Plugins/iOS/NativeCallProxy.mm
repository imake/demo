#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"


@implementation FrameworkLibAPI

id<NativeCallsProtocol> api = NULL;
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi
{
    api = aApi;
}

@end


extern "C" {
    void showHostMainWindow(const char* color) { return [api showHostMainWindow:[NSString stringWithUTF8String:color]]; }
}
extern "C" {
void sendPlatformStartSenseComplete() {
    return [api sendPlatformStartSenseComplete];
}
}

extern "C" {
void refreshWithBytes(void* bytes, UInt32 length){
    return [api refreshWithBytes:bytes length:length];
}
}

