#pragma endian little

#include <std/sys.pat>
#include <std/mem.pat>
#include <std/string.pat>

#define DSSS_MAGIC				"DSSS"
#define ENCRYPTION_ID			0x2
#define ENCRYPTION_TYPE			"None"

struct DSSS_FileHeader {
	char magic[std::string::length(DSSS_MAGIC)] [[color("33FF33")]];
	u32 unknown_1 [[color("FF3333")]];
	u32 encryptionType [[color("FFFF33")]];
	u32 unknown_2 [[color("FF3333")]];
};

struct DSSS_NoneFooter {
	u32 unknown_1 [[color("FF3333")]];
	u32 unknown_2 [[color("FF3333")]];
	u32 unknown_3 [[color("FF3333")]];
	u32 dataSlot [[color("33FF33")]];
	u32 murmurHash3Signature [[color("FF00AA")]];
};

fn footer_address() {
	return std::mem::size() - sizeof(DSSS_NoneFooter);
};

struct DSSS_NoneCtx {
	DSSS_FileHeader FileHeader;
	u32 steamID_32 [[color("44FFFF")]];
	DSSS_NoneFooter NoneFooter @ footer_address();
};

DSSS_NoneCtx DSSS @ 0x0;

// Assert file
std::assert(DSSS.FileHeader.magic == DSSS_MAGIC, "File is not a valid DSSS file!");
std::assert(DSSS.FileHeader.encryptionType == ENCRYPTION_ID, "DSSS file is not encrypted with " + ENCRYPTION_TYPE + " encryption!");

// Output
std::print("File Encryption: {};", ENCRYPTION_TYPE);
std::print("Data Slot: {};", DSSS.NoneFooter.dataSlot);
std::print("SteamID_32: {};", DSSS.steamID_32);
std::print("");