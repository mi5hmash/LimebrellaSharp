#pragma author Mi5hmasH
#pragma endian little

#include <std/sys.pat>
#include <std/mem.pat>
#include <std/string.pat>

#define DSSS_MAGIC       "DSSS"
#define ENCRYPTION_ID    0x2
#define ENCRYPTION_TYPE  "None"

struct DSSS_FileHeader {
    char magic[std::string::length(DSSS_MAGIC)] [[color("33FF33")]];
    u32 unknown_1 [[color("FF3333")]];
    u32 encryptionType [[color("FFFF33")]];
    u32 unknown_2 [[color("FF3333")]];
};

struct DSSS_AccountId {
    u32 accountId [[color("44FFFF")]];
    u32 _padding  [[color("555555")]];
};

struct DSSS_RawFooter {
    u32 dataSlot [[color("33FF33")]];
    u32 murmurHash3Signature [[color("FF00AA")]];
};

fn get_data_size() {
    return std::mem::size() - sizeof(DSSS_FileHeader) - sizeof(DSSS_AccountId) - sizeof(DSSS_RawFooter);
};

struct DSSS_RawCtx {
    DSSS_FileHeader FileHeader;
    DSSS_AccountId AccountId;
    u8 data[get_data_size()] [[color("2482C4")]];
    DSSS_RawFooter RawFooter;
};

DSSS_RawCtx DSSS @ 0x0;

// Assert file
std::assert(DSSS.FileHeader.magic == DSSS_MAGIC, "File is not a valid DSSS file!");
std::assert(DSSS.FileHeader.encryptionType == ENCRYPTION_ID, "DSSS file encryption type is not " + ENCRYPTION_TYPE + "!");

// Output
std::print("File Encryption: {};", ENCRYPTION_TYPE);
std::print("Data Slot: {};", DSSS.RawFooter.dataSlot);
std::print("AccountID: {};", DSSS.AccountId.accountId);
std::print("");