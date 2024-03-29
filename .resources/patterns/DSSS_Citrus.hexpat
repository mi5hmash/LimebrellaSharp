#pragma endian little

#include <std/sys.pat>
#include <std/mem.pat>
#include <std/string.pat>

#define DSSS_MAGIC				"DSSS"
#define ENCRYPTION_ID			0x4
#define ENCRYPTION_TYPE			"Citrus"
#define KEY_FRAGMENTS_PER_BANK	16

struct DSSS_FileHeader {
	char magic[std::string::length(DSSS_MAGIC)] [[color("33FF33")]];
	u32 unknown_1 [[color("FF3333")]];
	u32 encryptionType [[color("FFFF33")]];
	u32 unknown_2 [[color("FF3333")]];
};

struct CITRUS_PublicKeyBankHeader {
	u128 key[2] [[color("AA33FF")]];
};

struct CITRUS_PublicKeyFragment {
	u64 key[4] [[color("3388FF")]];
};

struct CITRUS_PublicKeyFragmentsBank {
	CITRUS_PublicKeyBankHeader bankHeader;
	CITRUS_PublicKeyFragment keyFragment[KEY_FRAGMENTS_PER_BANK];
};

struct CITRUS_Segment {
	CITRUS_PublicKeyFragmentsBank citrus_PublicKeyFragmentsBank;
	u8 segmentData[0x40000] [[color("FFFF33")]];
	u64 segmentChecksum[4] [[color("FF8800")]];
};

struct DSSS_CitrusFooter {
    u8 salt[0x1000] [[color("44FFFF")]];
	u64 decryptedDataLength [[color("885500")]];
	u32 murmurHash3Signature [[color("FF00AA")]];
};

fn segments_count() {
	return (std::mem::size() - sizeof(DSSS_FileHeader) - sizeof(DSSS_CitrusFooter)) / sizeof(CITRUS_Segment);
};

struct DSSS_CitrusCtx {
	DSSS_FileHeader FileHeader;
	CITRUS_Segment CitrusSegment[segments_count()];
	DSSS_CitrusFooter CitrusFooter;
};

DSSS_CitrusCtx DSSS @ 0x0;

// Assert file
std::assert(DSSS.FileHeader.magic == DSSS_MAGIC, "File is not a valid DSSS file!");
std::assert(DSSS.FileHeader.encryptionType == ENCRYPTION_ID, "DSSS file is not encrypted with " + ENCRYPTION_TYPE + " encryption!");

// Output
std::print("File Encryption: {};", ENCRYPTION_TYPE);
std::print("");