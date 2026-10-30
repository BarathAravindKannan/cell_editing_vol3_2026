# Pytest configuration for the harness's own self-tests.
#
# The harness code lives at .agent/tools/harness.py; it depends on
# `.agent/tools/features.py` (which in turn pins REPO_ROOT and AGENT_DIR
# from the repo layout). The Python tests under tests/python/ run with
# the repo root as the test rootdir (pytest auto-detection), so this
# conftest adds .agent/tools/ to sys.path before any test module is
# imported.
#
# No test under tests/python/ may mutate .agent/tasks/index.json,
# .agent/evidence/, or any lock file in place. Tests that need a
# synthetic lock should use a tmp_path fixture and point the harness's
# load_lock / cmd_lock at a copy in tmp; see test_cmd_lock_writes_portable_keys.

import os
import sys

# Repo root is the parent of tests/python/ (pytest sets rootdir there).
REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir))
TOOLS = os.path.join(REPO_ROOT, ".agent", "tools")
if TOOLS not in sys.path:
    sys.path.insert(0, TOOLS)
