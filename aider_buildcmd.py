#!/usr/bin/env python3
"""
A simple build command script for the SignalRMapRealtime repository.

Running this script will:
1. Restore NuGet packages.
2. Build the solution.
3. Run all unit tests.

It is intended to be invoked from the repository root:
    python3 ./aider_buildcmd.py
"""

import subprocess
import sys
from pathlib import Path

def run_cmd(command: list[str], cwd: Path | None = None) -> int:
    """Run a command and stream its output."""
    result = subprocess.run(command, cwd=cwd, stdout=sys.stdout, stderr=sys.stderr)
    return result.returncode

def main() -> int:
    repo_root = Path(__file__).resolve().parent

    # Step 1: dotnet restore
    print("\n=== Restoring NuGet packages ===")
    if run_cmd(["dotnet", "restore"], cwd=repo_root) != 0:
        print("dotnet restore failed.", file=sys.stderr)
        return 1

    # Step 2: dotnet build (Release configuration)
    print("\n=== Building the solution ===")
    if run_cmd(["dotnet", "build", "--configuration", "Release"], cwd=repo_root) != 0:
        print("dotnet build failed.", file=sys.stderr)
        return 1

    # Step 3: dotnet test
    print("\n=== Running unit tests ===")
    if run_cmd(["dotnet", "test", "--configuration", "Release", "--no-build"], cwd=repo_root) != 0:
        print("dotnet test failed.", file=sys.stderr)
        return 1

    print("\nAll steps completed successfully.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
