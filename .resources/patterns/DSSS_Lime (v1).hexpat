#pragma endian little

#include <std/sys.pat>
#include <std/mem.pat>
#include <std/string.pat>

#define DSSS_MAGIC  	       "DSSS"
#define ENCRYPTION_ID           0x10
#define ENCRYPTION_TYPE 		"Lime (v1)"
#define KEY_FRAGMENTS_PER_BANK  4

struct DSSS_FileHeader {
    char magic[std::string::length(DSSS_MAGIC)] [[color("33FF33")]];
    u32 unknown_1 [[color("FF3333")]];
    u32 encryptionType [[color("FFFF33")]];
    u32 unknown_2 [[color("FF3333")]];
};

struct LIME_PublicKeyHeader {
    u64 header[4] [[color("AA33FF")]];
    u64 _padding[4] [[color("555555")]];
};

struct LIME_PublicKeyFragment {
    u64 public_key_fragment[5] [[color("3388FF")]];
    u64 _padding[3] [[color("555555")]];
};

struct LIME_PublicKeyFragmentsBank {
    LIME_PublicKeyHeader bankHeader;
    LIME_PublicKeyFragment keyFragment;
};

struct LIME_Segment {
    LIME_PublicKeyFragmentsBank keyFragmentsBank[KEY_FRAGMENTS_PER_BANK];
    u8 segmentData[0x1000] [[color("FFFF33")]];
    u64 segmentChecksum[4] [[color("FF8800")]];
};

struct DSSS_LimeFooter {
    u8 salt[0x80] [[color("44FFFF")]];
    u64 decryptedDataLength [[color("885500")]];
    u32 murmurHash3Signature [[color("FF00AA")]];
};

fn segments_count() {
    return (std::mem::size() - sizeof(DSSS_FileHeader) - sizeof(DSSS_LimeFooter)) / sizeof(LIME_Segment);
};

struct DSSS_LimeCtx {
    DSSS_FileHeader FileHeader;
    LIME_Segment LimeSegment[segments_count()];
    DSSS_LimeFooter LimeFooter;
};

DSSS_LimeCtx DSSS @ 0x0;

// Assert file
std::assert(DSSS.FileHeader.magic == DSSS_MAGIC, "File is not a valid DSSS file!");
std::assert(DSSS.FileHeader.encryptionType == ENCRYPTION_ID, "DSSS file is not encrypted with " + ENCRYPTION_TYPE + " encryption!");
// Check Encryption Version
#define ENCRYPTION_VERSION_ERROR_MESSAGE "Encryption version seems to be greater than 1."
std::assert(DSSS.LimeSegment[0].keyFragmentsBank[0].bankHeader.header[0] == 0x5B49D50217C839BB, ENCRYPTION_VERSION_ERROR_MESSAGE);
std::assert(DSSS.LimeSegment[0].keyFragmentsBank[0].bankHeader.header[1] == 0x772BEEF5D2441867, ENCRYPTION_VERSION_ERROR_MESSAGE);
std::assert(DSSS.LimeSegment[0].keyFragmentsBank[0].bankHeader.header[2] == 0x6E236B076EEB11B8, ENCRYPTION_VERSION_ERROR_MESSAGE);
std::assert(DSSS.LimeSegment[0].keyFragmentsBank[0].bankHeader.header[3] == 0x1216F542E37CEE41, ENCRYPTION_VERSION_ERROR_MESSAGE);

// Output
std::print("File Encryption: {};", ENCRYPTION_TYPE);
std::print("Number of Segments: {};", segments_count());
std::print("");