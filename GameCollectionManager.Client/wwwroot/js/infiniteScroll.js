window.infiniteScrollHandlers = window.infiniteScrollHandlers || {};

window.setupInfiniteScrollById = (containerId, dotNetHelper, methodName) => {
    console.log(`[InfiniteScroll] Setting up for container: ${containerId}, method: ${methodName}`);
    
    if (window.infiniteScrollHandlers[containerId]) {
        console.log(`[InfiniteScroll] Cleaning up existing handler for ${containerId}`);
        window.infiniteScrollHandlers[containerId].dispose();
    }
    
    let attempts = 0;
    const maxAttempts = 10;
    const retryDelay = 300;
    
    const tryAttach = () => {
        attempts++;
        const outerContainer = document.getElementById(containerId);
        
        if (!outerContainer) {
            if (attempts < maxAttempts) {
                console.log(`[InfiniteScroll] Container ${containerId} not found (attempt ${attempts}/${maxAttempts}), retrying...`);
                setTimeout(tryAttach, retryDelay);
            } else {
                console.error(`[InfiniteScroll] ❌ Container ${containerId} not found after ${maxAttempts} attempts`);
            }
            return;
        }
        
        // Find the actual scrollable element inside MudDataGrid
        const scrollContainer = outerContainer.querySelector('.mud-table-container');
        
        if (!scrollContainer) {
            if (attempts < maxAttempts) {
                console.log(`[InfiniteScroll] Scrollable element not found in ${containerId} (attempt ${attempts}/${maxAttempts}), retrying...`);
                setTimeout(tryAttach, retryDelay);
            } else {
                console.error(`[InfiniteScroll] ❌ Scrollable element not found in ${containerId} after ${maxAttempts} attempts`);
            }
            return;
        }
        
        console.log(`[InfiniteScroll] ✅ Found scrollable container in ${containerId}:`, scrollContainer);
        
        setTimeout(() => {
            const dimensions = {
                scrollHeight: scrollContainer.scrollHeight,
                clientHeight: scrollContainer.clientHeight,
                offsetHeight: scrollContainer.offsetHeight,
                computedHeight: window.getComputedStyle(scrollContainer).height,
                overflow: window.getComputedStyle(scrollContainer).overflowY
            };
            
            console.log(`[InfiniteScroll] ${containerId} dimensions:`, dimensions);
            
            if (scrollContainer.scrollHeight <= scrollContainer.clientHeight) {
                console.warn(`[InfiniteScroll] ⚠ ${containerId} is NOT scrollable!`);
                console.warn(`   scrollHeight (${scrollContainer.scrollHeight}) <= clientHeight (${scrollContainer.clientHeight})`);
            } else {
                console.log(`[InfiniteScroll] ✅ ${containerId} is scrollable and ready`);
            }
        }, 500);
        
        let isLoading = false;
        const threshold = 300;
        
        const handleScroll = async () => {
            if (isLoading) return;
            
            const scrollTop = scrollContainer.scrollTop;
            const scrollHeight = scrollContainer.scrollHeight;
            const clientHeight = scrollContainer.clientHeight;
            const distanceFromBottom = scrollHeight - (scrollTop + clientHeight);
            
            console.log(`[InfiniteScroll] ${containerId} scrolled:`, {
                scrollTop: Math.round(scrollTop),
                scrollHeight,
                clientHeight,
                distanceFromBottom: Math.round(distanceFromBottom)
            });
            
            if (distanceFromBottom <= threshold) {
                console.log(`[InfiniteScroll] 🔥 Triggering ${methodName}`);
                isLoading = true;
                
                try {
                    await dotNetHelper.invokeMethodAsync(methodName);
                    console.log(`[InfiniteScroll] ✅ ${methodName} completed`);
                } catch (error) {
                    console.error(`[InfiniteScroll] ❌ Error calling ${methodName}:`, error);
                } finally {
                    setTimeout(() => {
                        isLoading = false;
                        console.log(`[InfiniteScroll] Ready for next load`);
                    }, 1000);
                }
            }
        };
        
        let scrollTimeout;
        const debouncedScroll = () => {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(handleScroll, 150);
        };
        
        scrollContainer.addEventListener('scroll', debouncedScroll, { passive: true });
        console.log(`[InfiniteScroll] ✅ Scroll listener attached to ${containerId}`);
        
        window.infiniteScrollHandlers[containerId] = {
            dispose: () => {
                console.log(`[InfiniteScroll] Disposing handler for ${containerId}`);
                scrollContainer.removeEventListener('scroll', debouncedScroll);
                clearTimeout(scrollTimeout);
                delete window.infiniteScrollHandlers[containerId];
            }
        };
        
        setTimeout(() => {
            console.log(`[InfiniteScroll] Testing ${containerId} - Try scrolling now!`);
            console.log(`[InfiniteScroll] Current position:`, {
                scrollTop: scrollContainer.scrollTop,
                scrollHeight: scrollContainer.scrollHeight,
                clientHeight: scrollContainer.clientHeight
            });
        }, 1000);
    };
    
    tryAttach();
};

// Manual test function
window.testScroll = (containerId) => {
    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container ${containerId} not found`);
        return;
    }
    
    console.log(`Testing ${containerId}:`, {
        scrollTop: container.scrollTop,
        scrollHeight: container.scrollHeight,
        clientHeight: container.clientHeight,
        canScroll: container.scrollHeight > container.clientHeight,
        computed: {
            height: window.getComputedStyle(container).height,
            overflow: window.getComputedStyle(container).overflowY,
            maxHeight: window.getComputedStyle(container).maxHeight
        }
    });
    
    // Try to scroll
    container.scrollTop = container.scrollHeight / 2;
    console.log('Attempted to scroll to middle. New scrollTop:', container.scrollTop);
};

// Cleanup all handlers
window.disposeAllInfiniteScroll = () => {
    Object.keys(window.infiniteScrollHandlers).forEach(key => {
        window.infiniteScrollHandlers[key].dispose();
    });
    console.log('[InfiniteScroll] All handlers disposed');
};

console.log('[InfiniteScroll] Script loaded successfully');