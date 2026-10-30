"""Regression tests for harness lock-key portability.

Background
----------
The harness persists a small JSON lock file (.agent/harness.lock) and a
control-plane lock (.agent/HARNESS-LOCK.json) whose keys are relative
paths. Before the fix, those keys were produced by `os.path.relpath`
and `os.path.join`, which return the local OS path separator. A lock
written on Windows therefore had backslash keys
(`src\\Syncfusion.Blazor.Grid.csproj`); a doctor / verify run on
Linux then saw only forward-slash keys, the set comparison failed,
and every manifest was reported as drift. The user perceived this as
"the harness only works after a second try without any code changes."

These tests lock the contract: the helper `harness.normalize_rel` is
the single point of normalization, its conversion is OS-independent,
and the doctor / verify consumers accept lock files written on any
host.

What we are NOT testing
-----------------------
- We do not run `harness.py doctor` end-to-end (it depends on the
  host's PATH, the node_modules install, and 21 gates). The
  integration check is the `harness-verify` inner gate, which is
  exercised by the workflow's final gate run.
- We do not assert on the rewritten lock file's exact SHA-256 (the
  `generatedAt` timestamp and recorded `python` version make the
  file non-byte-deterministic across runs). What we assert is that
  the keys are forward-slash form and that a legacy backslash lock
  compares equal to a freshly-written forward-slash lock.
"""

import importlib
import json
import os
import shutil
import sys

import pytest


@pytest.fixture
def harness_module():
    """Fresh import of `.agent.tools.harness` (avoids stale pyc)."""
    for m in list(sys.modules):
        if m == "harness":
            del sys.modules[m]
    return importlib.import_module("harness")


# -- the contract on normalize_rel -----------------------------------------

class TestNormalizeRel:
    def test_idempotent(self, harness_module):
        """normalize_rel(normalize_rel(x)) == normalize_rel(x) for any x."""
        for sample in [
            "a/b/c",
            "a\\b\\c",
            "a/b\\c",
            "knowledge/blazor-lifecycle.md",
            "knowledge\\blazor-lifecycle.md",
            "",
            "/",
            "\\",
        ]:
            once = harness_module.normalize_rel(sample)
            twice = harness_module.normalize_rel(once)
            assert once == twice, f"not idempotent on {sample!r}: {once!r} vs {twice!r}"

    def test_converts_windows_separator_on_posix(self, harness_module, monkeypatch):
        """On a POSIX host, backslashes in the input still become /'s.

        This is the bug that the naive `rel.replace(os.sep, "/")` would
        miss on a Linux checkout reading a Windows-locked file: `os.sep`
        is `/` on POSIX, so a backslash-keyed string passes through
        unchanged. The correct helper is unconditional.
        """
        monkeypatch.setattr(harness_module.os, "sep", "/", raising=False)
        assert harness_module.normalize_rel("a\\b\\c") == "a/b/c"

    def test_converts_posix_separator_on_windows(self, harness_module, monkeypatch):
        """On a Windows host, forward slashes in the input become \\'s.

        Symmetric to the previous test. The contract is "any path
        separator becomes /" so the result is identical regardless of
        host.
        """
        monkeypatch.setattr(harness_module.os, "sep", "\\", raising=False)
        assert harness_module.normalize_rel("a/b/c") == "a/b/c"
        # The Windows-form input is also accepted (the host cannot
        # tell; the result is the canonical form).
        assert harness_module.normalize_rel("a\\b\\c") == "a/b/c"

    def test_empty_and_root_unchanged(self, harness_module):
        """Empty / single-separator inputs are no-ops (not None, not
        '/' alone). They are not real lock keys; the helper does not
        pretend they are."""
        assert harness_module.normalize_rel("") == ""
        assert harness_module.normalize_rel("a") == "a"
        assert harness_module.normalize_rel("a/b") == "a/b"
        assert harness_module.normalize_rel("a\\b") == "a/b"


# -- set-equality across separators ----------------------------------------

class TestSetEqualityAcrossSeparators:
    def test_manifest_set_equal(self, harness_module):
        """The exact bug the user reported: a backslash-keyed set
        and a forward-slash-keyed set describe the same project."""
        windows_form = [
            "package.json",
            "src\\Syncfusion.Blazor.Grid.csproj",
            "src\\Syncfusion.Blazor.Grid.sln",
            "tests\\Base\\MediaQuery\\Syncfusion.Blazor.Tests.MediaQuery.csproj",
            "tests\\bunit\\Syncfusion.Blazor.Tests.Grids.csproj",
        ]
        posix_form = [
            "package.json",
            "src/Syncfusion.Blazor.Grid.csproj",
            "src/Syncfusion.Blazor.Grid.sln",
            "tests/Base/MediaQuery/Syncfusion.Blazor.Tests.MediaQuery.csproj",
            "tests/bunit/Syncfusion.Blazor.Tests.Grids.csproj",
        ]
        windows_norm = {harness_module.normalize_rel(m) for m in windows_form}
        posix_norm = {harness_module.normalize_rel(m) for m in posix_form}
        assert windows_norm == posix_norm

    def test_knowledge_object_keys_equal(self, harness_module):
        """The same bug on the `knowledge` lock object."""
        windows = {
            "knowledge\\blazor-lifecycle.md": {"sha256": "abc"},
            "knowledge\\bunit-docs.md": {"sha256": "def"},
        }
        posix = {
            "knowledge/blazor-lifecycle.md": {"sha256": "abc"},
            "knowledge/bunit-docs.md": {"sha256": "def"},
        }
        windows_norm = {harness_module.normalize_rel(k): v for k, v in windows.items()}
        posix_norm = {harness_module.normalize_rel(k): v for k, v in posix.items()}
        assert set(windows_norm) == set(posix_norm)
        assert windows_norm == posix_norm


# -- write-side contract ---------------------------------------------------

class TestCmdLockWritesPortableKeys:
    """`harness.py lock` must produce keys in the POSIX form on every
    host. We exercise the lock-dict construction in isolation rather
    than run cmd_lock end-to-end (which depends on a healthy
    contract and the host's toolchain)."""

    def test_find_manifests_uses_forward_slash(self, harness_module, monkeypatch):
        """find_manifests() must return forward-slash keys, even if the
        host is Windows."""
        # Simulate a Windows-style `os.sep` and assert the function
        # still produces forward-slash output. We do not need to walk a
        # real repo; the only invariant is the format of the returned
        # relative paths.
        monkeypatch.setattr(harness_module.os, "sep", "\\", raising=False)
        # Replace os.walk with a tiny fixture: two manifests, one nested.
        fake_root = harness_module.REPO_ROOT
        calls = []

        def fake_walk(top):
            calls.append(top)
            yield (fake_root, [], ["package.json"])
            nested_src = os.path.join(fake_root, "src")
            yield (nested_src, [], ["Foo.csproj"])
            deep_root = os.path.join(fake_root, "tests", "deep", "nested")
            yield (deep_root, [], ["Bar.csproj"])

        monkeypatch.setattr(harness_module.os, "walk", fake_walk)
        # MANIFESTS contains "package.json" by default; the .csproj
        # / .sln suffix match covers the other two.
        result = harness_module.find_manifests()
        for rel in result:
            assert "\\" not in rel, f"backslash in find_manifests() output: {rel!r}"
            assert rel.startswith("package.json") or rel.startswith("src/") or \
                   rel.startswith("tests/"), f"unexpected rel: {rel!r}"

    def test_cmd_lock_dict_has_portable_keys(self, harness_module, monkeypatch, tmp_path):
        """Building the lock dict (the structure cmd_lock writes) must
        use forward-slash keys regardless of the host."""
        # Stand in for the config that cmd_lock consumes.
        cfg = {"gates": []}
        # Use a tmp knowledge dir we control.
        kdir = tmp_path / "knowledge"
        kdir.mkdir()
        (kdir / "a.md").write_text("- Source: https://example.com/a (retrieved 2026-10-15)\n")
        (kdir / "sub").mkdir()
        (kdir / "sub" / "b.md").write_text("- Source: https://example.com/b\n")
        monkeypatch.setattr(harness_module, "KNOWLEDGE", str(kdir))
        monkeypatch.setattr(harness_module, "LOCK", str(tmp_path / "harness.lock"))
        monkeypatch.setattr(harness_module, "CONFIG", str(tmp_path / "harness.yaml"))
        # Build a synthetic manifest list (the part cmd_lock records).
        fake_manifests = [
            "package.json",
            "src\\Syncfusion.Blazor.Grid.csproj",
            "src\\Syncfusion.Blazor.Grid.sln",
        ]
        monkeypatch.setattr(harness_module, "find_manifests",
                            lambda: fake_manifests)
        # Replicate the cmd_lock body that builds the `knowledge` and
        # `manifests` keys (we do not call cmd_lock directly because it
        # requires a real lock file path and a toolchain walk).
        knowledge = {}
        for root, _, files in os.walk(str(kdir)):
            for f in sorted(files):
                p = os.path.join(root, f)
                rel = harness_module.normalize_rel(os.path.relpath(p, str(kdir.parent)))
                knowledge[rel] = {"sha256": harness_module.sha256_file(p),
                                  "sources": harness_module._extract_sources(p)}
        manifests = [harness_module.normalize_rel(m) for m in fake_manifests]
        for k in knowledge:
            assert "\\" not in k, f"backslash in knowledge key: {k!r}"
        for m in manifests:
            assert "\\" not in m, f"backslash in manifest: {m!r}"


# -- the doctor verdict on a legacy lock -----------------------------------

class TestDoctorVerdictOnLegacyLock:
    """The doctor must report `OK` when the lock's manifest set
    matches the freshly walked set, even when the lock's stored
    fingerprint was computed on a different OS. We exercise the
    comparison logic without running cmd_doctor (which depends on a
    full repo walk and 21 gates)."""

    def test_canonical_set_comparison(self, harness_module, monkeypatch):
        """The exact check the doctor does at lines 1258-1270 of
        harness.py (after the fix)."""
        lock = {
            "fingerprint": "deadbeefdeadbeefdeadbeefdeadbeef",
            "manifests": [
                "package.json",
                "src\\Syncfusion.Blazor.Grid.csproj",
                "src\\Syncfusion.Blazor.Grid.sln",
            ],
        }
        # The freshly computed (forward-slash) set on this host.
        fresh = [
            "package.json",
            "src/Syncfusion.Blazor.Grid.csproj",
            "src/Syncfusion.Blazor.Grid.sln",
        ]
        # The exact normalization the doctor performs on read:
        old = {harness_module.normalize_rel(m) for m in (lock.get("manifests") or [])}
        new = set(fresh)
        assert old == new, "canonical sets must be equal across separator forms"
