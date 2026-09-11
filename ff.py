import functools
from abc import ABC, abstractmethod
import base64
import enum

import zlib
from collections.abc import Iterable

import itertools


class ItemType(enum.IntEnum):
    Empty = -1
    Void = 0
    Blue = 1
    White = 2
    Yellow = 3
    Green = 4
    List = 5


class Axis(enum.IntEnum):
    Z = 0
    X = 1
    Y = 2


def stable_combine(a: int, b: int) -> int:
    def u64(x: int) -> int:
        return x & ((1 << 64) - 1)

    a, b = u64(a), u64(b)
    value = u64(0xcbf29ce484222325 ^ a)
    value = u64(value * 0x0100000001B3)
    value = u64(value ^ b)
    value = u64(value * 0x0100000001B3)
    return value


class Item(ABC):
    SHARABLE_KEY_VERSION = 2

    @property
    @abstractmethod
    def hash(self) -> int:
        pass

    @abstractmethod
    def _add_to_references(self, references: dict[int, tuple[int]]):
        pass

    def to_sharable_key(self) -> str:
        references = {}
        self._add_to_references(references)

        data = bytearray()

        def write_ulong(num: int):
            data.extend(num.to_bytes(length=8, byteorder="little", signed=False))

        write_ulong(self.hash)
        write_ulong(len(references))
        for key, value in references.items():
            write_ulong(key)
            write_ulong(len(value))
            for item in value:
                write_ulong(item)

        compressor = zlib.compressobj(method=zlib.DEFLATED, level=6, wbits=-zlib.MAX_WBITS)
        data = bytearray(compressor.compress(data) + compressor.flush())
        data.insert(0, self.SHARABLE_KEY_VERSION)
        data = (base64.urlsafe_b64encode(data)
                .rstrip(b"=")
                .replace(b"_", b"_1")
                .replace(b"-", b"_0"))
        return data.decode('ascii')


class UnitItem(Item):
    def __init__(self, type: ItemType):
        self.type = type
        super().__init__()

    @property
    def hash(self) -> int:
        return int(self.type)

    def _add_to_references(self, references: dict[int, tuple[int]]):
        references[self.hash] = (self.type,)


class ListItem(Item):
    def __init__(self, axis: Axis, items: Iterable[Item | ItemType], structural_hash: int | None = None):
        self.axis = axis
        self.items = tuple(item if isinstance(item, Item) else UnitItem(item) for item in items)
        if structural_hash is None:
            structural_hash = functools.reduce(stable_combine, (item.hash for item in self.items))
        self.structural_hash: int = structural_hash

        self._hash: int | None = None

        super().__init__()

    @property
    def hash(self) -> int:
        if self._hash is None:
            h = stable_combine(ItemType.List, self.axis)
            h = stable_combine(h, self.structural_hash)
            self._hash = functools.reduce(stable_combine, (item.hash for item in self.items), initial=h)
        return self._hash

    def _add_to_references(self, references: dict[int, tuple[int]]):
        if self.hash not in references:
            for item in self.items:
                item._add_to_references(references)
            references[self.hash] = (int(self.axis), self.structural_hash) + tuple(item.hash for item in self.items)

    @classmethod
    def rod(cls, axis: Axis, shape: Item | ItemType, n: int):
        return cls(axis, itertools.repeat(shape, n))

