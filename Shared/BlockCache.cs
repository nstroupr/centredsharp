﻿using System.Collections.Concurrent;

namespace CentrED; 

public class BlockCache {

    private readonly ConcurrentDictionary<int, Block> _blocks;
    private readonly ConcurrentQueue<int> _queue;
    private int _maxSize;
    public BlockChanged? OnRemovedCachedObject;

    public BlockCache(int maxSize = 256) {
        _maxSize = maxSize;
        _queue = new ConcurrentQueue<int>();
        _blocks = new ConcurrentDictionary<int, Block>();
    }

    public void Add(Block block) {
        var blockId = BlockId(block.LandBlock.X, block.LandBlock.Y);
        _blocks.TryAdd(blockId, block);
        _queue.Enqueue(blockId);
        if (_blocks.Count > _maxSize) {
            Dequeue();
        }
    }

    public void Clear() {
        while (!_queue.IsEmpty) {
            Dequeue();
        }
    }

    public bool Contains(ushort x, ushort y) {
        return Get(BlockId(x, y)) != null;
    }

    public Block? Get(ushort x, ushort y) {
        return Get(BlockId(x, y));
    }
    
    public Block? Get(int blockId) {
        _blocks.TryGetValue(blockId, out Block? value);
        return value;
    }

    private Block? Dequeue() {
        if (!_queue.TryDequeue(out var blockId)) return null;
        if (!_blocks.TryRemove(blockId, out Block? dequeued)) return null;
        OnRemovedCachedObject?.Invoke(dequeued);
        return dequeued;
    }
    
    public static int BlockId(ushort x, ushort y) {
        return HashCode.Combine(x, y);
    }

    public void Resize(int newSize) {
        _maxSize = newSize;
        while (_blocks.Count > _maxSize) {
            Dequeue();
        }
    }
}