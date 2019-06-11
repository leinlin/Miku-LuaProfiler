LOCAL_PATH := $(call my-dir)
TARGET_PLATFORM := android-22
include $(CLEAR_VARS)
LOCAL_ARM_MODE := arm
LOCAL_MODULE := mikuhooker 
LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../src/include

LOCAL_CPPFLAGS := -O2
LOCAL_CFLAGS :=  -O2 -std=gnu99
LOCAL_CFLAGS += -DMIKU_HOOKER_ANDROID

LOCAL_SRC_FILES := ../../src/source/mikuhooker.c \
  ../../src/source/inlineHook.c \
 ../../src/source/relocate.c \
 
include $(BUILD_SHARED_LIBRARY)