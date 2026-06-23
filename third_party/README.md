# Third-party source

This directory contains third-party source code vendored with GitDesk.

## leveldb.net

- Source: https://github.com/oodrive/leveldb.net
- Commit: 3048ef454a4ba82fa126a53054ef50a91ac7358c
- License: Apache License 2.0, see `leveldb.net/license.txt`
- Purpose: source for the `LevelDB.Standard` dependency used by `Services/WorkspaceStore.cs`.

The application still references the `LevelDB.Standard` NuGet package for build and runtime native asset resolution. The vendored copy is kept so the LevelDB wrapper and native LevelDB source are available inside this repository.
