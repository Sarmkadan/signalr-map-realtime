#!/usr/bin/env python3
"""
Utility script for the Aider tool.

Usage:
    python3 aider_buildcmd.py <command> [args...]

The script changes to the repository root and executes the given command
using subprocess.run, forwarding the exit code. It is intentionally minimal
and has no external dependencies.
"""

import os
import sys
import subprocess

def run_cmd(command: list[str]) -> int:
    """Run the given command list and return its exit code."""
    if not command:
        print("No command provided.", file=sys.stderr)
        return 1
    try:
        result = subprocess.run(command, check=False)
        return result.returncode
    except FileNotFoundError:
        print(f"Command not found: {command[0]}", file=sys.stderr)
        return 127
    except Exception as exc:
        print(f"Error executing command: {exc}", file=sys.stderr)
        return 1

def main() -> int:
    """Entry point."""
    # Change to the repository root
    repo_root = "/home/redrocket/task-factory/workdir/signalr-map-realtime"
    try:
        os.chdir(repo_root)
    except Exception as exc:
        print(f"Failed to change directory to repo root: {exc}", file=sys.stderr)
        return 1

    # The script expects the command to execute after the script name.
    # Example: python aider_buildcmd.py dotnet test
    cmd = sys.argv[1:]
    return run_cmd(cmd)

if __name__ == "__main__":
    sys.exit(main())
