TOP=..
include $(TOP)/Make.config

# without this many compiler warnings about unused functions and variables
# in system headers show up.
export CCACHE_CPP2=1

#
# Common
#

SHARED_INC += \
	delegates.inc                              \

SHIPPED_HEADERS +=         \
	xamarin/mono-runtime.h \
	xamarin/xamarin.h      \
	xamarin/main.h         \
	xamarin/trampolines.h  \
	xamarin/runtime.h      \
	xamarin/runtime-generated.h \

SHARED_SOURCES += mono-runtime.m bindings.m bindings-generated.m shared.m runtime.m trampolines.m trampolines-invoke.m xamarin-support.m nsstring-localization.m trampolines-varargs.m
SHARED_I386_SOURCES += trampolines-i386.m trampolines-i386-asm.s trampolines-i386-objc_msgSend.s trampolines-i386-objc_msgSendSuper.s trampolines-i386-objc_msgSend_stret.s trampolines-i386-objc_msgSendSuper_stret.s
SHARED_X86_64_SOURCES += trampolines-x86_64.m trampolines-x86_64-asm.s trampolines-x86_64-objc_msgSend.s trampolines-x86_64-objc_msgSendSuper.s trampolines-x86_64-objc_msgSend_stret.s trampolines-x86_64-objc_msgSendSuper_stret.s
SHARED_ARM64_SOURCES += trampolines-arm64.m trampolines-arm64-asm.s
SHARED_HEADERS += shared.h product.h delegates.h runtime-internal.h $(SHARED_INC) $(SHIPPED_HEADERS) trampolines-internal.h slinked-list.h

SHARED_FILES = $(SHARED_SOURCES) $(SHARED_HEADERS) $(SHARED_I386_SOURCES) $(SHARED_X86_64_SOURCES) $(SHARED_ARM64_SOURCES)

EXTRA_DEPENDENCIES = $(SHARED_HEADERS) $(TOP)/Make.config $(TOP)/mk/mono.mk

ifdef INJECT_X86_64_SLICE
X86_64_SLICE=$(abspath $(CURDIR)/x86-64-slice.dylib)
$(X86_64_SLICE): Makefile
	$(Q) echo "void xamarin_x86_64_function_for_notarization_workaround_v2 () {}" | $(MAC_CC) -shared -x c -o$@ -
else
X86_64_SLICE=
endif

xamarin/mono-runtime.h: mono-runtime.h.t4 exports.t4
	$(Q_GEN) $(TT) $< -o $@

xamarin/runtime-generated.h: runtime-generated.h.t4 delegates.t4
	$(Q_GEN) $(TT) $< -o $@

mono-runtime.m: mono-runtime.m.t4 exports.t4
	$(Q_GEN) $(TT) $< -o $@

delegates.%: delegates.%.t4 delegates.t4
	$(Q_GEN) $(TT) $< -o $@

Delegates.generated.cs: Delegates.cs.t4 delegates.t4
	$(Q_GEN) $(TT) $< -o $@

bindings-generator.exe: bindings-generator.cs
	$(Q) $(SYSTEM_CSC) $< -out:$@ -debug:full -features:strict 

bindings-generated.m: bindings-generator.exe
	$(Q_GEN) $(SYSTEM_MONO) --debug $< $@

# our makefiles don't support building the same source file multiple times with different defines,
# so just symlink extension-main.m to another file and compile that instead.
app-main.m watchextension-main.m tvextension-main.m: extension-main.m
	$(Q_LN) ln -fs $< $@

#
# MonoTouch defines (used for all MonoTouch platforms: iOS, WatchOS and TVOS)
#

MONOTOUCH_SOURCES =     \
	monotouch-debug.m   \
	monotouch-main.m    \

MONOTOUCH_HEADERS =     \
    monotouch-debug.h   \

MONOTOUCH_LIBS =               \
	libextension.a             \
	libtvextension.a           \
	libapp.a                   \
	libxamarin.a               \
	libxamarin-debug.a         \
	libxamarin.dylib           \
	libxamarin-debug.dylib     \

MONOTOUCH_FRAMEWORKS =  \
	Xamarin             \
	Xamarin-debug       \

MONOTOUCH_SOURCE_STEMS = $(patsubst %.s,%,$(patsubst %.m,%,$(SHARED_SOURCES) $(MONOTOUCH_SOURCES)))
MONOTOUCH_I386_SOURCE_STEMS = $(patsubst %.s,%,$(patsubst %.m,%,$(SHARED_I386_SOURCES)))
MONOTOUCH_X86_64_SOURCE_STEMS = $(patsubst %.s,%,$(patsubst %.m,%,$(SHARED_X86_64_SOURCES)))
MONOTOUCH_ARM64_SOURCE_STEMS = $(patsubst %.s,%,$(patsubst %.m,%,$(SHARED_ARM64_SOURCES)))

MONOTOUCH_X86_SOURCE_STEMS = $(MONOTOUCH_I386_SOURCE_STEMS)

#
# FrameworkTemplate contains the install targets and sets up some variables for frameworks
#

define FrameworkTemplate

$(2)_ARCHITECTURES = $(3)
$(2)_FRAMEWORKS    = $(MONOTOUCH_FRAMEWORKS)

RUNTIME_$(2)_TARGETS_DIRS +=                                        \
	$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin.framework \
	$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin-debug.framework \

RUNTIME_$(2)_TARGETS +=                                                                                \
	$$(foreach file,$$($(2)_FRAMEWORKS),$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/$$(file).framework/$$(file))   \
	$$(foreach file,$$($(2)_FRAMEWORKS),$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/$$(file).framework/Info.plist) \
	$$(foreach file,$$($(2)_FRAMEWORKS),$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/$$(file).framework.dSYM/Contents/Info.plist) \

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin.framework/Xamarin: .libs/$(1)/Xamarin.framework | $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin.framework
	$(Q) $(CP) $$< $$@

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin-debug.framework/Xamarin-debug: .libs/$(1)/Xamarin-debug.framework | $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin-debug.framework
	$(Q) $(CP) $$< $$@

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/%.framework/Info.plist: Xamarin.framework-$(1).Info.plist | $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/%.framework
	$(Q) sed 's/@BUNDLE_EXECUTABLE@/$$*/' $$< > $$@

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin.framework.dSYM/Contents/Info.plist: $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin.framework/Xamarin $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin.framework/Info.plist
	$$(Q_GEN) dsymutil -j 4 $$< -o $$(abspath $$(dir $$<)/..)/Xamarin.framework.dSYM

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin-debug.framework.dSYM/Contents/Info.plist: $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin-debug.framework/Xamarin-debug $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/Frameworks/Xamarin-debug.framework/Info.plist
	$$(Q_GEN) dsymutil -j 4 $$< -o $$(abspath $$(dir $$<)/..)/Xamarin-debug.framework.dSYM

.libs/$(1)/Xamarin.framework:  $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/Xamarin.$$(arch).framework)
	$(Q) rm -f $$@
ifeq (1,$$(words $$($(2)_ARCHITECTURES)))
	$(Q) $(CP) $$^ $$@
else
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@
endif

.libs/$(1)/Xamarin-debug.framework:  $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/Xamarin-debug.$$(arch).framework)
	$(Q) rm -f $$@
ifeq (1,$$(words $$($(2)_ARCHITECTURES)))
	$(Q) $(CP) $$^ $$@
else
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@
endif
endef

# 1: platform
# 2: variable prefix
# 3: architectures
ifdef INCLUDE_IOS
ifdef INCLUDE_DEVICE
$(eval $(call FrameworkTemplate,iphoneos,IPHONEOS,armv7 armv7s arm64))
endif
$(eval $(call FrameworkTemplate,iphonesimulator,IOSSIMULATOR,x86 x86_64))
endif

ifdef INCLUDE_WATCH
ifdef INCLUDE_DEVICE
$(eval $(call FrameworkTemplate,watchos,WATCHOS,armv7k arm64_32))
endif
$(eval $(call FrameworkTemplate,watchsimulator,WATCHSIMULATOR,x86))
endif

ifdef INCLUDE_TVOS
ifdef INCLUDE_DEVICE
$(eval $(call FrameworkTemplate,tvos,TVOS,arm64))
endif
$(eval $(call FrameworkTemplate,tvsimulator,TVSIMULATOR,x86_64))
endif

#
# PlatformTemplate contains the install targets and sets up some variables
#

define PlatformTemplate

$(2)_SOURCES       = $(MONOTOUCH_SOURCES)
$(2)_HEADERS       = $(MONOTOUCH_HEADERS)
$(2)_SOURCE_STEMS  = $(MONOTOUCH_SOURCE_STEMS)
$(2)_X86_64_SOURCE_STEMS = $(MONOTOUCH_X86_64_SOURCE_STEMS)
$(2)_I386_SOURCE_STEMS = $(MONOTOUCH_I386_SOURCE_STEMS)
$(2)_ARM64_SOURCE_STEMS = $(MONOTOUCH_ARM64_SOURCE_STEMS)
$(2)_LIBRARIES     = $(MONOTOUCH_LIBS)
$(2)_ARCHITECTURES = $(3)

RUNTIME_$(2)_TARGETS_DIRS +=                                        \
	$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/lib                 \
	$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/include/xamarin     \

RUNTIME_$(2)_TARGETS +=                                                                                \
	$$(foreach file,$$($(2)_LIBRARIES),$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/lib/$$(file))       \
	$(foreach file,$(SHIPPED_HEADERS),$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/include/$(file))     \

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/lib/%.a: .libs/$(1)/%.a | $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/lib
	$(Q) install -m 0644 $$< $$@

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/lib/%.dylib: .libs/$(1)/%.dylib | $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/lib
	$$(Q_STRIP) $(DEVICE_BIN_PATH)/bitcode_strip $$< -m -o $$@

$(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/include/%.h: %.h | $(IOS_DESTDIR)$$(XAMARIN_$(2)_SDK)/include/xamarin
	$(Q) install -m 0644 $$< $$@

$$(RUNTIME_$(2)_TARGETS_DIRS):
	$(Q) mkdir -p $$@

all-$(2):: $$(RUNTIME_$(2)_TARGETS)

ifdef INCLUDE_XAMARIN_LEGACY
all-local:: $$(RUNTIME_$(2)_TARGETS)
install-local:: $$(RUNTIME_$(2)_TARGETS)
endif

endef

# 1: platform
# 2: variable prefix
# 3: architectures
ifdef INCLUDE_IOS
ifdef INCLUDE_DEVICE
$(eval $(call PlatformTemplate,iphoneos,IPHONEOS,armv7 armv7s arm64))
endif
$(eval $(call PlatformTemplate,iphonesimulator,IOSSIMULATOR,x86 x86_64))
endif
ifdef INCLUDE_MACCATALYST
$(eval $(call PlatformTemplate,maccatalyst,MACCATALYST,x86_64))
endif

ifdef INCLUDE_WATCH
ifdef INCLUDE_DEVICE
$(eval $(call PlatformTemplate,watchos,WATCHOS,armv7k arm64_32))
endif
$(eval $(call PlatformTemplate,watchsimulator,WATCHSIMULATOR,x86 x86_64))
endif

ifdef INCLUDE_TVOS
ifdef INCLUDE_DEVICE
$(eval $(call PlatformTemplate,tvos,TVOS,arm64))
endif
$(eval $(call PlatformTemplate,tvsimulator,TVSIMULATOR,x86_64))
endif

#
# LibTemplate we build two different libraries from the same source code,
# libapp.a and libextension.a
#
# They're all built from the same soure file (extension-main.m), but with different defines:
# libextension.a has EXTENSION defined.
#

define LibTemplate
$$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/extension-main.$$(arch).o): EXTRA_DEFINES=-DEXTENSION
$$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/watchextension-main.$$(arch).o): EXTRA_DEFINES=-DWATCH_EXTENSION
$$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/tvextension-main.$$(arch).o): EXTRA_DEFINES=-DTV_EXTENSION

.libs/$(1)/libextension.%.a: .libs/$(1)/extension-main.%.o
	$(Q) rm -f $$@
	$$(call Q_2,AR,    [$1]) $(DEVICE_BIN_PATH)/ar cru $$@ $$^

.libs/$(1)/libapp.%.a:       .libs/$(1)/app-main.%.o
	$(Q) rm -f $$@
	$$(call Q_2,AR,    [$1]) $(DEVICE_BIN_PATH)/ar cru $$@ $$^

.libs/$(1)/libextension.a: $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/libextension.$$(arch).a)
	$(Q) rm -f $$@
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@

.libs/$(1)/libapp.a:       $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/libapp.$$(arch).a)
	$(Q) rm -f $$@
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@

.libs/$(1)/libtvextension.%.a: .libs/$(1)/tvextension-main.%.o
	$(Q) rm -f $$@
	$$(call Q_2,AR,    [$1]) $(DEVICE_BIN_PATH)/ar cru $$@ $$^

.libs/$(1)/libtvextension.a: $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/libtvextension.$$(arch).a)
	$(Q) rm -f $$@
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@

.libs/$(1)/libxamarin.dylib:  $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/libxamarin.$$(arch).dylib)
	$(Q) rm -f $$@
ifeq (1,$$(words $$($(2)_ARCHITECTURES)))
	$(Q) $(CP) $$^ $$@
else
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@
endif
	$(Q) install_name_tool -id @rpath/libxamarin.dylib $$@
	$(Q) install_name_tool -change @rpath/ @rpath/libmonosgen-2.0.dylib $$@

.libs/$(1)/libxamarin-debug.dylib:  $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/libxamarin-debug.$$(arch).dylib)
	$(Q) rm -f $$@
ifeq (1,$$(words $$($(2)_ARCHITECTURES)))
	$(Q) $(CP) $$^ $$@
else
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@
endif
	$(Q) install_name_tool -id @rpath/libxamarin-debug.dylib $$@
	$(Q) install_name_tool -change @rpath/ @rpath/libmonosgen-2.0.dylib $$@

.SECONDARY: $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/app-main.$$(arch).o)
endef

$(eval $(call LibTemplate,iphoneos,IPHONEOS))
$(eval $(call LibTemplate,iphonesimulator,IOSSIMULATOR))
$(eval $(call LibTemplate,maccatalyst,MACCATALYST))
$(eval $(call LibTemplate,watchos,WATCHOS))
$(eval $(call LibTemplate,watchsimulator,WATCHSIMULATOR))
$(eval $(call LibTemplate,tvos,TVOS))
$(eval $(call LibTemplate,tvsimulator,TVSIMULATOR))

#
# LibXamarinTemplate (and LibXamarinArchTemplate) builds libxamarin.a
#

define LibXamarinArchTemplate
.libs/$(1)/libxamarin$(4).$(5).a: $$($(5)_$(1)$(3)_OBJECTS)
	$$(Q) rm -f $$@
	$$(call Q_2,AR,    [$1]) $(DEVICE_BIN_PATH)/ar Scru $$@ $$^
	$$(call Q_2,RANLIB,[$1]) $(DEVICE_BIN_PATH)/ranlib -no_warning_for_no_symbols -q $$@
endef

define LibXamarinTemplate

$(1)$(3)_COMMON_DYLIB_FLAGS = -lmonosgen-2.0 -Wl,-install_name,libxamarin$(4).dylib -framework Foundation -framework CFNetwork -framework UIKit -lz
$(1)$(3)_COMMON_FRAMEWORK_FLAGS = -framework Mono -Wl,-install_name,@rpath/Xamarin$(4).framework/Xamarin$(4) -framework Foundation -framework CFNetwork -framework UIKit -lz

x86_$(1)$(3)_OBJECTS    = $$(patsubst %,.libs/$(1)/%$(4).x86.o,$$($(2)_SOURCE_STEMS))    $$(patsubst %,.libs/$(1)/%$(4).x86.o,$$($(2)_I386_SOURCE_STEMS))
x86_64_$(1)$(3)_OBJECTS = $$(patsubst %,.libs/$(1)/%$(4).x86_64.o,$$($(2)_SOURCE_STEMS)) $$(patsubst %,.libs/$(1)/%$(4).x86_64.o,$$($(2)_X86_64_SOURCE_STEMS))
armv7_$(1)$(3)_OBJECTS  = $$(patsubst %,.libs/$(1)/%$(4).armv7.o,$$($(2)_SOURCE_STEMS))
armv7s_$(1)$(3)_OBJECTS = $$(patsubst %,.libs/$(1)/%$(4).armv7s.o,$$($(2)_SOURCE_STEMS))
armv7k_$(1)$(3)_OBJECTS = $$(patsubst %,.libs/$(1)/%$(4).armv7k.o,$$($(2)_SOURCE_STEMS))
arm64_$(1)$(3)_OBJECTS  = $$(patsubst %,.libs/$(1)/%$(4).arm64.o,$$($(2)_SOURCE_STEMS))  $$(patsubst %,.libs/$(1)/%$(4).arm64.o,$$($(2)_ARM64_SOURCE_STEMS))
arm64_32_$(1)$(3)_OBJECTS = $$(patsubst %,.libs/$(1)/%$(4).arm64_32.o,$$($(2)_SOURCE_STEMS))

$$(foreach arch,$$($(2)_ARCHITECTURES),$$(eval $$(call LibXamarinArchTemplate,$(1),$(2),$(3),$(4),$$(arch))))

.libs/$(1)/libxamarin$(4).a: $$(foreach arch,$$($(2)_ARCHITECTURES),.libs/$(1)/libxamarin$(4).$$(arch).a)
	$(Q) rm -f $$@
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@

.libs/$(1)/libxamarin$(4).x86.dylib: EXTRA_FLAGS=$$($(1)$(3)_COMMON_DYLIB_FLAGS)
.libs/$(1)/libxamarin$(4).x86.dylib: $$(x86_$(1)$(3)_OBJECTS)

.libs/$(1)/libxamarin$(4).x86_64.dylib: EXTRA_FLAGS=$$($(1)$(3)_COMMON_DYLIB_FLAGS)
.libs/$(1)/libxamarin$(4).x86_64.dylib: $$(x86_64_$(1)$(3)_OBJECTS)

.libs/$(1)/libxamarin$(4).armv7.dylib: EXTRA_FLAGS=$$($(1)$(3)_COMMON_DYLIB_FLAGS) -miphoneos-version-min=8.0
.libs/$(1)/libxamarin$(4).armv7.dylib: $$(armv7_$(1)$(3)_OBJECTS)

.libs/$(1)/libxamarin$(4).armv7s.dylib: EXTRA_FLAGS=$$($(1)$(3)_COMMON_DYLIB_FLAGS) -miphoneos-version-min=8.0
.libs/$(1)/libxamarin$(4).armv7s.dylib: $$(armv7s_$(1)$(3)_OBJECTS)

.libs/$(1)/libxamarin$(4).arm64.dylib: EXTRA_FLAGS=$$($(1)$(3)_COMMON_DYLIB_FLAGS)
.libs/$(1)/libxamarin$(4).arm64.dylib: $$(arm64_$(1)$(3)_OBJECTS)

.libs/$(1)/libxamarin$(4).armv7k.dylib: EXTRA_FLAGS=$$($(1)$(3)_COMMON_DYLIB_FLAGS)
.libs/$(1)/libxamarin$(4).armv7k.dylib: $$(armv7k_$(1)$(3)_OBJECTS)

.libs/$(1)/libxamarin$(4).arm64_32.dylib: EXTRA_FLAGS=$$($(1)$(3)_COMMON_DYLIB_FLAGS)
.libs/$(1)/libxamarin$(4).arm64_32.dylib: $$(arm64_32_$(1)$(3)_OBJECTS)

.libs/$(1)/Xamarin$(4).x86.framework: EXTRA_FLAGS=$$($(1)$(3)_COMMON_FRAMEWORK_FLAGS)
.libs/$(1)/Xamarin$(4).x86.framework: $$(x86_$(1)$(3)_OBJECTS)

.libs/$(1)/Xamarin$(4).x86_64.framework: EXTRA_FLAGS=$$($(1)$(3)_COMMON_FRAMEWORK_FLAGS)
.libs/$(1)/Xamarin$(4).x86_64.framework: $$(x86_64_$(1)$(3)_OBJECTS)

.libs/$(1)/Xamarin$(4).armv7.framework: EXTRA_FLAGS=$$($(1)$(3)_COMMON_FRAMEWORK_FLAGS) -miphoneos-version-min=8.0
.libs/$(1)/Xamarin$(4).armv7.framework: $$(armv7_$(1)$(3)_OBJECTS)

.libs/$(1)/Xamarin$(4).armv7s.framework: EXTRA_FLAGS=$$($(1)$(3)_COMMON_FRAMEWORK_FLAGS) -miphoneos-version-min=8.0
.libs/$(1)/Xamarin$(4).armv7s.framework: $$(armv7s_$(1)$(3)_OBJECTS)

.libs/$(1)/Xamarin$(4).arm64.framework: EXTRA_FLAGS=$$($(1)$(3)_COMMON_FRAMEWORK_FLAGS)
.libs/$(1)/Xamarin$(4).arm64.framework: $$(arm64_$(1)$(3)_OBJECTS)

.libs/$(1)/Xamarin$(4).armv7k.framework: EXTRA_FLAGS=$$($(1)$(3)_COMMON_FRAMEWORK_FLAGS)
.libs/$(1)/Xamarin$(4).armv7k.framework: $$(armv7k_$(1)$(3)_OBJECTS)

.libs/$(1)/Xamarin$(4).arm64_32.framework: EXTRA_FLAGS=$$($(1)$(3)_COMMON_FRAMEWORK_FLAGS)
.libs/$(1)/Xamarin$(4).arm64_32.framework: $$(arm64_32_$(1)$(3)_OBJECTS)
endef

$(eval $(call LibXamarinTemplate,iphoneos,IPHONEOS))
$(eval $(call LibXamarinTemplate,iphoneos,IPHONEOS,_DEBUG,-debug))
$(eval $(call LibXamarinTemplate,iphonesimulator,IOSSIMULATOR))
$(eval $(call LibXamarinTemplate,iphonesimulator,IOSSIMULATOR,_DEBUG,-debug))
$(eval $(call LibXamarinTemplate,maccatalyst,MACCATALYST))
$(eval $(call LibXamarinTemplate,maccatalyst,MACCATALYST,_DEBUG,-debug))
$(eval $(call LibXamarinTemplate,watchos,WATCHOS))
$(eval $(call LibXamarinTemplate,watchos,WATCHOS,_DEBUG,-debug))
$(eval $(call LibXamarinTemplate,watchsimulator,WATCHSIMULATOR))
$(eval $(call LibXamarinTemplate,watchsimulator,WATCHSIMULATOR,_DEBUG,-debug))
$(eval $(call LibXamarinTemplate,tvos,TVOS))
$(eval $(call LibXamarinTemplate,tvos,TVOS,_DEBUG,-debug))
$(eval $(call LibXamarinTemplate,tvsimulator,TVSIMULATOR))
$(eval $(call LibXamarinTemplate,tvsimulator,TVSIMULATOR,_DEBUG,-debug))

#
# Xamarin.Mac
#

MAC_ARCHITECTURES = x86_64

CLANG_ARCH = $(addprefix -arch ,$(MAC_ARCHITECTURES))
MAC_CLANG = DEVELOPER_DIR=$(XCODE_DEVELOPER_ROOT) $(MAC_CC) -mmacosx-version-min=$(MIN_OSX_SDK_VERSION)

MAC_SHIPPED_HEADERS = xamarin/launch.h

MAC_STATIC_CFLAGS = $(MAC_CFLAGS)

MAC_SOURCES = $(SHARED_SOURCES) $(SHARED_X86_64_SOURCES) launcher.m

ALLOWED_UNDEFINED_SYMBOLS = _xamarin_enable_debug _xammac_setup

MAC_LIBS =                   \
	libextension.a			 \
	libxammac.a              \
	libxammac-debug.a        \
	libxammac-classic.a      \
	libxammac-classic-debug.a\
	libxammac.dylib          \
	libxammac-debug.dylib    \
	libxammac-system.a       \
	libxammac-system-debug.a \
	libxammac-system-classic.a       \
	libxammac-system-classic-debug.a \

#
# libxammac[-debug].a: (no defines)
#    This is linked into the native executable when embedding the Mono runtime.
#
# libxammac[-debug].dylib: -DDYNAMIC_MONO_RUNTIME -DDYLIB
#    This is not officially supported. It's used when not wanting to use mmp for
#    whatever reason (usually to make build processes easier). There is no good
#    reason to keep using it, since mmp now supports creating empty 'shell' apps
#    with no assemblies, which the user can fill in as they want.
#
# libxammac-system[-debug].a: -DDYNAMIC_MONO_RUNTIME
#    This is used linked into the native executable when using the system Mono (i.e. not)
#    embedding the Mono runtime).
#
# libxamarin-coreclr.a:
#    This is used when using the CoreCLR runtime instead of Mono.
#    CORECLR_RUNTIME is defined for these versions of libxamarin.
#

ifdef INCLUDE_XAMARIN_LEGACY
all-local:: $(TARGETS)
endif

define ObjTemplate
DYNAMIC_DYLIB$(2)_OBJECTS   = $$(foreach src,$$(MAC_SOURCES),.libs/mac/$$(basename $$(src))$(3).dylib.$(1).o)
STATIC_LAUNCHER$(2)_OBJECTS = $$(foreach src,$$(MAC_SOURCES),.libs/mac/$$(basename $$(src))$(3).static.$(1).o)
SYSTEM_LAUNCHER$(2)_OBJECTS = $$(foreach src,$$(MAC_SOURCES),.libs/mac/$$(basename $$(src))$(3).system.$(1).o)

.libs/mac/%$(3).dylib.$(1).o: %.m $$(SHARED_HEADERS) | .libs/mac
	$$(call Q_2,OBJC,  [mac]) $(MAC_CLANG) -arch $(1) $(4) -c $$(MAC_OBJC_CFLAGS) $$(MAC_CFLAGS) -DDYNAMIC_MONO_RUNTIME -DDYLIB -o $$@ $$<

.libs/mac/%$(3).dylib.$(1).o: %.s $$(SHARED_HEADERS) | .libs/mac
	$$(call Q_2,ASM,   [mac]) $(MAC_CLANG) -arch $(1) $(4) -c $$(MAC_CFLAGS) -DDYNAMIC_MONO_RUNTIME -DDYLIB -o $$@ $$<

.libs/mac/%$(3).static.$(1).o: %.s $$(SHARED_HEADERS) | .libs/mac
	$$(call Q_2,ASM,   [mac]) $(MAC_CLANG) -arch $(1) $(4) -c $$(MAC_STATIC_CFLAGS) -o $$@ $$<

.libs/mac/%$(3).static.$(1).o: %.m $$(SHARED_HEADERS) | .libs/mac
	$$(call Q_2,OBJC,  [mac]) $(MAC_CLANG) -arch $(1) $(4) $$(MAC_OBJC_CFLAGS) -c $$(MAC_STATIC_CFLAGS) -o $$@ $$<

.libs/mac/%$(3).system.$(1).o: %.m $$(SHARED_HEADERS) | .libs/mac
	$$(call Q_2,OBJC,  [mac]) $(MAC_CLANG) -arch $(1) $(4) $$(MAC_OBJC_CFLAGS) -c $$(MAC_CFLAGS) -DDYNAMIC_MONO_RUNTIME -o $$@ $$<

.libs/mac/%$(3).system.$(1).o: %.s $$(SHARED_HEADERS) | .libs/mac
	$$(call Q_2,ASM,   [mac]) $(MAC_CLANG) -arch $(1) $(4) -c $(MAC_CFLAGS) -DDYNAMIC_MONO_RUNTIME -o $$@ $$<

.libs/mac/libxammac$(3).$(1).dylib: $$(DYNAMIC_DYLIB$(2)_OBJECTS)
	$$(call Q_2,LD,    [mac]) $(MAC_CLANG) -arch $(1) -dynamiclib $$(MAC_LDFLAGS) -Wl,-install_name,libxammac$(3).dylib -o $$@ $$^ $$(addprefix -Xlinker -U -Xlinker ,$$(ALLOWED_UNDEFINED_SYMBOLS))

.libs/mac/libxammac$(3).$(1).a: $$(STATIC_LAUNCHER$(2)_OBJECTS)
	$$(call Q_2,LIB,   [mac]) xcrun libtool -no_warning_for_no_symbols -static -o $$@ $$^

.libs/mac/libxammac-system$(3).$(1).a: $$(SYSTEM_LAUNCHER$(2)_OBJECTS)
	$$(call Q_2,LIB,   [mac]) xcrun libtool -no_warning_for_no_symbols -static -o $$@ $$^

endef

$(eval $(call ObjTemplate,x86_64,64,,,64))
$(eval $(call ObjTemplate,x86_64,DEBUG64,-debug,-DDEBUG,64))

$(foreach arch,$(MAC_ARCHITECTURES),.libs/mac/extension-main.$(arch).o): EXTRA_DEFINES=-DEXTENSION

.libs/mac/extension-main.%.o: | .libs/mac
	$(Q) rm -f $@
	$(call Q_2,CC,  [mac]) $(MAC_CLANG) -c -DEXTENSION extension-main.m -arch $* -o $@

.libs/mac/libextension.%.a: .libs/mac/extension-main.%.o
	$(Q) rm -f $@
	$(call Q_2,AR,    [mac]) $(DEVICE_BIN_PATH)/ar cru $@ $^

.libs/mac/libextension.a: $(foreach arch,$(MAC_ARCHITECTURES),.libs/mac/libextension.$(arch).a)
	$(Q) rm -f $@
	$(call Q_2,LIPO,  [mac]) $(DEVICE_BIN_PATH)/lipo $^ -create -output $@

.libs/mac/libxammac-debug.dylib: .libs/mac/libxammac-debug.x86_64.dylib
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

.libs/mac/libxammac.dylib: .libs/mac/libxammac.x86_64.dylib
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

.libs/mac/libxammac-debug.a: .libs/mac/libxammac-debug.x86_64.a
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

.libs/mac/libxammac.a: .libs/mac/libxammac.x86_64.a
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

.libs/mac/libxammac-system-debug.a: .libs/mac/libxammac-system-debug.x86_64.a
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

.libs/mac/libxammac-system.a: .libs/mac/libxammac-system.x86_64.a
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

.libs/mac/libxammac-%.a: $(MACIOS_BINARIES_PATH)/libxammac-%.a | .libs/mac
	$(Q) $(CP) $< $@

.libs/mac/libxamarin%.a: .libs/mac/libxamarin%.x86_64.a
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

.libs/mac/libxamarin%.dylib: .libs/mac/libxamarin%.x86_64.dylib
	$(call Q_2,LIPO,  [mac]) xcrun lipo -create $^ -o $@

RUNTIME_MAC_TARGETS_DIRS +=                                    \
	$(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib             \
	$(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib/mono        \
	$(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/lib                     \
	$(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/include/xamarin         \

RUNTIME_MAC_TARGETS +=                                                                                \
	$(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib/XamMacLauncher                                     \
	$(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib/mono/XamMacLauncher                                \
	$(foreach file,$(SHIPPED_HEADERS),$(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/include/$(file))             \
	$(foreach file,$(MAC_SHIPPED_HEADERS),$(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/include/$(file))         \
	$(foreach file,$(MAC_LIBS),$(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/lib/$(file))                        \

# The XamMacLauncher file must exist for VSfM to be able to open XM/Classic projects (so that people can use the migration wizard)
$(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib/XamMacLauncher: | $(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib
	$(Q) touch $@

$(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/lib/%: .libs/mac/% | $(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/lib
	$(Q) $(CP) $< $@

$(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib/mono/XamMacLauncher: | $(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR)/lib/mono
	$(Q) ln -sF ../XamMacLauncher $@

$(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/include/%.h: %.h | $(MAC_DESTDIR)$(XAMARIN_MACOS_SDK)/include/xamarin
	$(Q) install -m 0644 $< $@

$(RUNTIME_MAC_TARGETS_DIRS):
	$(Q) mkdir -p $@

ifdef INCLUDE_XAMARIN_LEGACY
all-local:: $(RUNTIME_MAC_TARGETS)
install-local:: $(RUNTIME_MAC_TARGETS)
endif

#
# .NET
#

DOTNET_iOS_LIBRARIES = libapp.a libextension.a
DOTNET_tvOS_LIBRARIES = libtvextension.a
DOTNET_watchOS_LIBRARIES = libextension.a libwatchextension.a
DOTNET_macOS_LIBRARIES = libextension.a

define DotNetLibTemplate
DOTNET_TARGETS += \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet.dylib \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet-debug.dylib \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet.a \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet-debug.a \
	$$(foreach lib,$$(DOTNET_$(2)_LIBRARIES),$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/$$(lib)) \
	$$(foreach header,$$(SHIPPED_HEADERS),$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/$$(header)) \

DOTNET_TARGET_DIRS += \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/xamarin \

$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/xamarin/%.h: xamarin/%.h | $(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/xamarin
	$$(Q) $$(CP) $$< $$@
endef

$(foreach platform,$(DOTNET_PLATFORMS),$(foreach rid,$(DOTNET_$(platform)_RUNTIME_IDENTIFIERS),$(eval $(call DotNetLibTemplate,$(platform),$(rid)))))

define DotNetCoreClrLibTemplate
DOTNET_TARGETS += \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet-coreclr.a \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet-coreclr-debug.a \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet-coreclr.dylib \
	$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-dotnet-coreclr-debug.dylib \

endef

$(foreach platform,$(DOTNET_CORECLR_PLATFORMS),$(foreach rid,$(DOTNET_$(platform)_RUNTIME_IDENTIFIERS),$(eval $(call DotNetCoreClrLibTemplate,$(platform),$(rid)))))

# a few lookup tables, because the data we have is not always in the format we need it

DOTNET_ios_x64_ARCHITECTURES=x86_64
DOTNET_ios_x86_ARCHITECTURES=x86
DOTNET_ios_arm_ARCHITECTURES=armv7 armv7s
DOTNET_ios_arm64_ARCHITECTURES=arm64
DOTNET_tvos_x64_ARCHITECTURES=x86_64
DOTNET_tvos_arm64_ARCHITECTURES=arm64
DOTNET_osx_x64_ARCHITECTURES=x86_64
DOTNET_osx_arm64_ARCHITECTURES=arm64
DOTNET_maccatalyst_x64_ARCHITECTURES=x86_64
DOTNET_maccatalyst_arm64_ARCHITECTURES=arm64

DOTNET_ios_x64_SDK_PLATFORM=iphonesimulator
DOTNET_ios_x86_SDK_PLATFORM=iphonesimulator
DOTNET_ios_arm_SDK_PLATFORM=iphoneos
DOTNET_ios_arm64_SDK_PLATFORM=iphoneos
DOTNET_tvos_x64_SDK_PLATFORM=tvsimulator
DOTNET_tvos_arm64_SDK_PLATFORM=tvos
DOTNET_osx_x64_SDK_PLATFORM=mac
DOTNET_osx_arm64_SDK_PLATFORM=mac
DOTNET_maccatalyst_x64_SDK_PLATFORM=maccatalyst
DOTNET_maccatalyst_arm64_SDK_PLATFORM=maccatalyst

DOTNET_iOS_SDK_PLATFORMS=iphonesimulator iphoneos
DOTNET_tvOS_SDK_PLATFORMS=tvsimulator tvos
DOTNET_macOS_SDK_PLATFORMS=mac
DOTNET_MacCatalyst_SDK_PLATFORMS=maccatalyst

DOTNET_iphonesimulator_DYLIB_FLAGS=-lmonosgen-2.0 -framework UIKit
DOTNET_iphoneos_DYLIB_FLAGS=-lmonosgen-2.0 -framework UIKit
DOTNET_tvsimulator_DYLIB_FLAGS=-lmonosgen-2.0 -framework UIKit
DOTNET_tvos_DYLIB_FLAGS=-lmonosgen-2.0 -framework UIKit
DOTNET_maccatalyst_DYLIB_FLAGS=-lmonosgen-2.0 -framework UIKit
DOTNET_mac_DYLIB_FLAGS=-lcoreclr

#
# DotNetInstallLibTemplate lipos or copies libraries into the destination directories
#

define DotNetInstallLibTemplate
$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-%.dylib: $$(foreach arch,$(3),.libs/$(4)/libxamarin-%.$$(arch).dylib) | $(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native
ifeq (1,$$(words $(3)))
	$(Q) $(CP) $$^ $$@
else
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@
endif
	$(Q) install_name_tool -id @rpath/libxamarin-$$*.dylib $$@

$(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native/libxamarin-%.a: $$(foreach arch,$(3),.libs/$(4)/libxamarin-%.$$(arch).a) | $(DOTNET_DESTDIR)/Microsoft.$(1).Runtime.$(2)/runtimes/$(2)/native
ifeq (1,$$(words $(3)))
	$(Q) $(CP) $$^ $$@
else
	$$(call Q_2,LIPO,  [$1]) $(DEVICE_BIN_PATH)/lipo $$^ -create -output $$@
endif
endef

# foreach (var platform in DOTNET_PLATFORMS)
#   foreach (var rid in DOTNET_<platform>_RUNTIME_IDENTIFIERS)
#     call DotNetInstallLibTemplate (platform, rid, architectures_for_rid, sdk_platform_for_rid)
$(foreach platform,$(DOTNET_PLATFORMS),$(foreach rid,$(DOTNET_$(platform)_RUNTIME_IDENTIFIERS),$(eval $(call DotNetInstallLibTemplate,$(platform),$(rid),$(DOTNET_$(shell echo $(rid) | tr -- - _)_ARCHITECTURES),$(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM)))))

#
# LibXamarinTemplate builds libxamarin.a
#

# The tvOS runtime pack doesn't ship support for bitcode yet, so linking fails. In the meantime, link with the old libmonosgen-2.0.dylib from the mono archive
# https://github.com/dotnet/runtime/issues/48508
DOTNET_tvos_arm64_LIBDIR=$(TOP)/builds/mono-ios-sdk-destdir/ios-libs/tvos

# The runtime pack is different for macOS/Mono (it has '.Mono' at the end), so add a special case here.
DOTNET_osx_x64_LIBDIR=$(TOP)/builds/downloads/microsoft.netcore.app.runtime.mono.osx-x64/$(BUNDLED_NETCORE_PLATFORMS_PACKAGE_VERSION)/runtimes/osx-x64/native
DOTNET_osx_arm64_LIBDIR=$(TOP)/builds/downloads/microsoft.netcore.app.runtime.mono.osx-arm64/$(BUNDLED_NETCORE_PLATFORMS_PACKAGE_VERSION)/runtimes/osx-arm64/native

define DotNetLibXamarinTemplate

DOTNET_$(4)_LIBDIR ?= $$(TOP)/builds/downloads/microsoft.netcore.app.runtime.$(3)/$(BUNDLED_NETCORE_PLATFORMS_PACKAGE_VERSION)/runtimes/$(3)/native
DOTNET_$(4)_DYLIB_FLAGS = $(DOTNET_$(1)_DYLIB_FLAGS) -Wl,-install_name,libxamarin$(7).dylib -framework Foundation -framework CFNetwork -lz -L$(abspath $(DOTNET_$(4)_LIBDIR))

DOTNET_$(4)_$(5)$(6)_OBJECTS   = $$(patsubst %,.libs/$(1)/%$(7).$(5).o,   $(MONOTOUCH_SOURCE_STEMS)) $$(patsubst %,.libs/$(1)/%$(7).$(5).o,   $(MONOTOUCH_$(shell echo $(5) | tr a-z A-Z)_SOURCE_STEMS))

.libs/$(1)/libxamarin$(7).$(5).a: $$(DOTNET_$(4)_$(5)$(6)_OBJECTS)
	$$(Q) rm -f $$@
	$$(call Q_2,AR,    [$1]) $(DEVICE_BIN_PATH)/ar Scru $$@ $$^
	$$(call Q_2,RANLIB,[$1]) $(DEVICE_BIN_PATH)/ranlib -no_warning_for_no_symbols -q $$@

.libs/$(1)/libxamarin$(7).$(5).dylib: EXTRA_FLAGS=$$(DOTNET_$(4)_DYLIB_FLAGS)
.libs/$(1)/libxamarin$(7).$(5).dylib: $$(DOTNET_$(4)_$(5)$(6)_OBJECTS)

endef

# foreach (var platform in DOTNET_PLATFORMS)
#   foreach (var rid in DOTNET_<platform>_RUNTIME_IDENTIFIERS))
#      foreach (var arch in DOTNET_<rid>_ARCHITECTURES)
#                                           1             2        3          4              5      6        7
#        call DotNetLibXamarinTemplate (platform, SDK_PLATFORM, rid, rid_with_underscores, arch,       , "-dotnet")
#        call DotNetLibXamarinTemplate (platform, SDK_PLATFORM, rid, rid_with_underscores, arch, _DEBUG, "-dotnet-debug")
#        call DotNetLibXamarinTemplate (platform, SDK_PLATFORM, rid, rid_with_underscores, arch, _CORECLR,       "-dotnet-coreclr")
#        call DotNetLibXamarinTemplate (platform, SDK_PLATFORM, rid, rid_with_underscores, arch, _CORECLR_DEBUG, "-dotnet-coreclr-debug")

$(foreach platform,$(DOTNET_PLATFORMS),$(foreach rid,$(DOTNET_$(platform)_RUNTIME_IDENTIFIERS),$(foreach arch,$(DOTNET_$(shell echo $(rid) | tr -- - _)_ARCHITECTURES),$(eval $(call DotNetLibXamarinTemplate,$(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM),$(shell echo $(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM) | tr a-z A-Z),$(rid),$(shell echo $(rid) | tr -- - _),$(arch),,-dotnet)))))
$(foreach platform,$(DOTNET_PLATFORMS),$(foreach rid,$(DOTNET_$(platform)_RUNTIME_IDENTIFIERS),$(foreach arch,$(DOTNET_$(shell echo $(rid) | tr -- - _)_ARCHITECTURES),$(eval $(call DotNetLibXamarinTemplate,$(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM),$(shell echo $(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM) | tr a-z A-Z),$(rid),$(shell echo $(rid) | tr -- - _),$(arch),_DEBUG,-dotnet-debug)))))
$(foreach platform,$(DOTNET_PLATFORMS),$(foreach rid,$(DOTNET_$(platform)_RUNTIME_IDENTIFIERS),$(foreach arch,$(DOTNET_$(shell echo $(rid) | tr -- - _)_ARCHITECTURES),$(eval $(call DotNetLibXamarinTemplate,$(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM),$(shell echo $(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM) | tr a-z A-Z),$(rid),$(shell echo $(rid) | tr -- - _),$(arch),_CORECLR,-dotnet-coreclr)))))
$(foreach platform,$(DOTNET_PLATFORMS),$(foreach rid,$(DOTNET_$(platform)_RUNTIME_IDENTIFIERS),$(foreach arch,$(DOTNET_$(shell echo $(rid) | tr -- - _)_ARCHITECTURES),$(eval $(call DotNetLibXamarinTemplate,$(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM),$(shell echo $(DOTNET_$(shell echo $(rid) | tr -- - _)_SDK_PLATFORM) | tr a-z A-Z),$(rid),$(shell echo $(rid) | tr -- - _),$(arch),_CORECLR_DEBUG,-dotnet-coreclr-debug)))))

dotnet: $(DOTNET_TARGETS)

$(DOTNET_TARGET_DIRS):
	$(Q) mkdir -p $@

ifdef ENABLE_DOTNET
all-local:: $(DOTNET_TARGETS)
install-local:: $(DOTNET_TARGETS)
endif

#
# Common
#

clean-local::
	$(Q) rm -f *.o
	$(Q) rm -f *.a
	$(Q) rm -Rf .libs
	$(Q) rm -f xamarin/mono-runtime.h mono-runtime.m delegates.h delegates.inc Delegates.generated.cs


include $(TOP)/mk/rules.mk

.SECONDARY: delegates.h delegates.inc
