{
  description = "BinaryDifference - Avalonia cross-platform binary diff tool";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs =
    {
      self,
      nixpkgs,
      flake-utils,
    }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = nixpkgs.legacyPackages.${system};

        nativeLibs = with pkgs; [
          fontconfig
          libglvnd
          libxkbcommon
          libx11
          libice
          libsm
          libxext
          libxrandr
          libxcursor
          libxi
          libxinerama
        ];

        nativeLibPath = pkgs.lib.makeLibraryPath nativeLibs;
      in
      {
        devShells.default = pkgs.mkShell {
          name = "binarydifference";

          packages =
            with pkgs;
            [
              dotnet-sdk_10
            ]
            ++ nativeLibs;

          shellHook = ''
            export LD_LIBRARY_PATH="${nativeLibPath}:$LD_LIBRARY_PATH"
            export DOTNET_ROOT="${pkgs.dotnet-sdk_10}"
          '';
        };
      }
    );
}
