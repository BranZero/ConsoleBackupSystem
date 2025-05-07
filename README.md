# ConsoleBackupSystem

A Simple Command Line Backup System.

## About

The ConsoleBackupSystem is a lightweight and efficient command-line tool designed to help users back up files and directories. It supports various features such as selective backups, file compression, and the ability to ignore specific paths during the backup process.

## Features

- **Backup Files and Directories**: Easily back up files or entire directories.
- **Selective Backup Modes**:
  - `ForceCopy`: Always copy files regardless of changes.
  - `AllOrNone`: Copy all files in a directory only if any file has changed.
  - `None`: Copy files only if they are not present in prior backups.
- **Ignore Paths**: Specify paths to exclude from the backup process.
- **Incremental Backups**: Avoid redundant backups by checking prior backups.
- **Compressed Archives**: Backups are stored as compressed `.zip` files for efficient storage.

## How It Works

1. **Adding Paths**: Use the `add` command to specify files or directories to include in the backup. You can also define ignore paths for directories.
2. **Removing Paths**: Use the `remove` command to delete paths from the backup list.
3. **Listing Paths**: Use the `list` command to view all paths currently configured for backup.
4. **Performing Backups**: Use the `backup` command to create a backup. You can specify options like prior backups.

## Commands

### Add a Path
```bash 
add [-options] <path> [pathsToIgnore...]
```
#### Options
- -f: Force add the path even if it doesn't exist.
- -c: Use `ForceCopy` mode.
- -a: Use `AllOrNone` mode.
- Default: If no mode is specified, the `None` mode is used, which copies files only if they are not present or modified since prior backups.

### Remove a Path
```bash 
remove <path>
```

### List all paths currently configured for backup
```bash 
list
```

### Perform a Backup
```bash 
backup [-options] <destinationDirectory> [priorBackupDirectories...]
```
#### Options
- -n: Check for prior backups in `destinationDirectory`.
- -c: Check for prior backups in the same folder.

## Requirements
- .NET 8.0 Runtime
