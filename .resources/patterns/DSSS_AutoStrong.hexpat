#pragma endian little

#include <std/sys.pat>
#include <std/mem.pat>
#include <std/string.pat>

#define DSSS_MAGIC				"DSSS"
#define ENCRYPTION_ID			0x3
#define ENCRYPTION_TYPE			"AutoStrong"

struct DSSS_FileHeader {
	char magic[std::string::length(DSSS_MAGIC)] [[color("33FF33")]];
	u32 unknown_1 [[color("FF3333")]];
	u32 encryptionType [[color("FFFF33")]];
	u32 unknown_2 [[color("FF3333")]];
};

struct DSSS_AutoStrongFooter {
	u32 unknown_1 [[color("FF3333")]];
	u32 unknown_2 [[color("FF3333")]];
	u32 unknown_3 [[color("FF3333")]];
	u32 dataSlot [[color("33FF33")]];
	u32 signature [[color("FF00AA")]];
};

fn footer_address() {
	return std::mem::size() - sizeof(DSSS_AutoStrongFooter);
};

struct DSSS_AutoStrongCtx {
	DSSS_FileHeader FileHeader;
	u32 steamID_32 [[color("44FFFF")]];
	DSSS_AutoStrongFooter AutoStrongFooter @ footer_address();
};

DSSS_AutoStrongCtx DSSS @ 0x0;

// Assert file
std::assert(DSSS.FileHeader.magic == DSSS_MAGIC, "File is not a valid DSSS file!");
std::assert(DSSS.FileHeader.encryptionType == ENCRYPTION_ID, "DSSS file is not encrypted with " + ENCRYPTION_TYPE + " encryption!");

// Output
std::print("File Encryption: {};", ENCRYPTION_TYPE);
std::print("Data Slot: {};", DSSS.AutoStrongFooter.dataSlot);
std::print("");