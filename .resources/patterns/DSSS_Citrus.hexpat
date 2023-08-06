#pragma endian little

#include <std/sys.pat>
#include <std/mem.pat>
#include <std/string.pat>

#define DSSS_MAGIC				"DSSS"
#define ENCRYPTION_ID			0x4
#define ENCRYPTION_TYPE			"Citrus"

struct DSSS_FileHeader {
	char magic[std::string::length(DSSS_MAGIC)] [[color("33FF33")]];
	u32 unknown_1 [[color("FF3333")]];
	u32 encryptionType [[color("FFFF33")]];
	u32 unknown_2 [[color("FF3333")]];
};

struct DSSS_CitrusFooter {
	u64 decryptedDataLength [[color("885500")]];
	u32 signature [[color("FF00AA")]];
};

fn footer_address() {
	return std::mem::size() - sizeof(DSSS_CitrusFooter);
};

struct DSSS_CitrusCtx {
	DSSS_FileHeader FileHeader;
	DSSS_CitrusFooter CitrusFooter @ footer_address();
};

DSSS_CitrusCtx DSSS @ 0x0;

// Assert file
std::assert(DSSS.FileHeader.magic == DSSS_MAGIC, "File is not a valid DSSS file!");
std::assert(DSSS.FileHeader.encryptionType == ENCRYPTION_ID, "DSSS file is not encrypted with " + ENCRYPTION_TYPE + " encryption!");

// Output
std::print("File Encryption: {};", ENCRYPTION_TYPE);
std::print("");